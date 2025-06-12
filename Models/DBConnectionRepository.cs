using ViewMyAssetDev.Models;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Dapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Diagnostics;

namespace ViewMyAssetDev.Models
{
    public class DBConnectionRepository
    {
        private readonly string _connectionString;

        public DBConnectionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
         // dbo.Transaction에 추가 (변동된 자산 dbo.UserStock에 저장)
        // 추가하는 종목 없을 시 insert, 있을 시 update
        public async Task BuyTransaction(Transaction trans) 
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Transaction 테이블에 거래 내용 저장
                await connection.ExecuteAsync("insert into [Transaction] (Id, Symbol, Amount, [TransactionType], Price, [Type])  values (@Id, @Symbol, @Amount, @TransactionType, @Price, @Type)", new { Id = trans.Id, Symbol = trans.Symbol, Amount = trans.Amount, TransactionType = trans.TransactionType, Price = trans.Price, Type = trans.Type });

                string query = "SELECT * FROM UserStock WHERE Id = @Id AND Symbol = @Symbol";
                var stock = connection.QueryFirstOrDefault<UserStock>(query, new { Id = trans.Id, Symbol = trans.Symbol });

                if (stock != null) // 이미 보유한 주식일 때
                {
                    int newAmount = stock.Amount + trans.Amount;
                    double newTotalPurchasePrice = stock.TotalPurchasePrice + trans.Price * trans.Amount;

                    query = "update UserStock set Amount = @Amount, TotalPurchasePrice = @TotalPurchasePrice where Id = @Id and Symbol = @Symbol";

                    await connection.ExecuteAsync(query, new { Id = trans.Id, Symbol = trans.Symbol, Amount = newAmount, TotalPurchasePrice = newTotalPurchasePrice });
                }

                else // 보유하지 않은 주식일 때
                {
                    query = "insert into [UserStock] values (@Id, @Symbol, @Amount, @TotalPurchasePrice, @Type)";

                    double newTotalPurchasePrice = trans.Price * trans.Amount;

                    await connection.ExecuteAsync(query, new { Id = trans.Id, Symbol = trans.Symbol, Amount = trans.Amount, TotalPurchasePrice = newTotalPurchasePrice, Type = trans.Type});
                }
            }
        }

        public async Task SellTransaction(Transaction trans)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync("insert into [Transaction] (Id, Symbol, Amount, [TransactionType], Price, [Type])  values (@Id, @Symbol, @Amount, @TransactionType, @Price, @Type)", new { Id = trans.Id, Symbol = trans.Symbol, Amount = trans.Amount, TransactionType = trans.TransactionType, Price = trans.Price, Type = trans.Type });

                string query = "SELECT * FROM UserStock WHERE Id = @Id AND Symbol = @Symbol";
                var stock = connection.QueryFirstOrDefault<UserStock>(query, new { Id = trans.Id, Symbol = trans.Symbol });

                if (stock.Amount == trans.Amount) // 보유 수량 전부 매도 시
                {
                    query = "delete from [UserStock]  WHERE Id = @Id AND Symbol = @Symbol";

                    try
                    { await connection.ExecuteAsync(query, new { Id = trans.Id, Symbol = trans.Symbol }); }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    return;
                }

                if (stock != null)
                {
                    int newAmount = stock.Amount - trans.Amount;
                    double newTotalPurchasePrice = Math.Round(stock.TotalPurchasePrice * trans.Amount / stock.Amount, 2);

                    query = "update UserStock set Amount = @Amount, TotalPurchasePrice = @TotalPurchasePrice where Id = @Id and Symbol = @Symbol";


                    await connection.ExecuteAsync(query, new { Id = trans.Id, Symbol = trans.Symbol, Amount = newAmount, TotalPurchasePrice = newTotalPurchasePrice });
                }
            }
        }

        public List<UserStock> GetUserStockById(string id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var result = connection.Query<UserStock>("select * from [UserStock] where Id = @Id", new {Id = id}).ToList();

                return result;
            }
        }

        public int GetStockAmountByIdSymbol(string id, string symbol)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM UserStock WHERE Id = @Id AND Symbol = @Symbol";
                var stock = connection.QueryFirstOrDefault<UserStock>(query, new { Id = id, Symbol = symbol });

                if (stock != null) // 보유하고 있는 종목 일 경우
                    return stock.Amount;
                else
                    return 0;
            }
        }

        public bool SellAvailable(string id, string symbol, int amount)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM UserStock WHERE Id = @Id AND Symbol = @Symbol";

                var stock = connection.QueryFirstOrDefault<UserStock>(query, new {Id = id, Symbol = symbol });

                if (stock != null)
                {
                    if (stock.Amount >= amount)
                        return true;
                }

                return false;
            }
        }
    }
}
