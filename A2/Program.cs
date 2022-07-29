using System;
using System.Net;
using System.Text;
using System.Data.SqlClient;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading;

namespace A2Test
{
    class Program
    {
        static int recordsOnPage = 1000;
        static int page = 0;
        static string repository = @"C:\USERS\AKHOK\SOURCE\REPOS\A2\A2\DATABASE1.MDF";
        static int time = 1000 * 60 * 10;
        static void Main(string[] args)
        {
            Console.WriteLine("Парсер для https://www.lesegais.ru/open-area/deal");
            Console.WriteLine("Парсер запущен. Пожалуйста подождите...");
            while (true)
            {
                string connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={repository};Integrated Security=True;Connect Timeout=30";
                SqlConnection connection = new SqlConnection(connectionString);
                float raws = 0;
                connection.Open();
                while (true)
                {
                    List<WoodDeal> woodDeals = GetWoodDeals();
                    if (woodDeals.Count == 0)
                        break;
                    WriteToDB(woodDeals, connection);
                    raws += woodDeals.Count;
                    if (page != -1)
                        page++;
                    Console.WriteLine($"{page}-ая страница обработана.");
                    Console.WriteLine($"Количество обработаных строк: {raws}");
                }
                Console.WriteLine($"Все строки обработаны.");
                Console.WriteLine($"Повторный запуск через {time / 60000} минут");
                connection.Close();
                Thread.Sleep(time);
            }
        }

        private static void WriteToDB(List<WoodDeal> woodDeals, SqlConnection connection)
        {

            for (int i = 0; i < woodDeals.Count; i++)
            {
                WoodDeal woodDeal = woodDeals[i];
                if (CheckDB(woodDeal, connection) == false)
                {
                    AddWoodDeal(woodDeal, connection);
                    Console.WriteLine("Добавлена запись: " + woodDeal.dealNumber);
                }
            }
        }

        private static void AddWoodDeal(WoodDeal woodDeals, SqlConnection connection)
        {
            string query = $"INSERT INTO WoodDeals " +
                "(sellerName, sellerInn, buyerName, buyerInn, woodVolumeBuyer, woodVolumeSeller, dealDate, dealNumber) " +
                $"VALUES (@sellerName, @sellerInn, @buyerName, @buyerInn, @woodVolumeBuyer, @woodVolumeSeller, @dealDate, @dealNumber)";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("sellerName", CheckString(woodDeals.sellerName)));
            command.Parameters.Add(new SqlParameter("sellerInn", CheckString(woodDeals.sellerInn)));
            command.Parameters.Add(new SqlParameter("buyerName", CheckString(woodDeals.buyerName)));
            command.Parameters.Add(new SqlParameter("buyerInn", CheckString(woodDeals.buyerInn)));
            command.Parameters.Add(new SqlParameter("woodVolumeBuyer", woodDeals.woodVolumeBuyer));
            command.Parameters.Add(new SqlParameter("woodVolumeSeller", woodDeals.woodVolumeSeller));
            command.Parameters.Add(new SqlParameter("dealDate", CheckString(woodDeals.dealDate)));
            command.Parameters.Add(new SqlParameter("dealNumber", CheckString(woodDeals.dealNumber)));

            command.ExecuteNonQuery();
        }
        private static string CheckString(string str)
        {
            if (str != null)
                return str;
            else
                return "";
        }

        private static bool CheckDB(WoodDeal woodDeals, SqlConnection connection)
        {
            string query = $"SELECT dealNumber FROM WoodDeals " +
            "WHERE dealNumber =  @dealNumber ";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add(new SqlParameter("dealNumber", woodDeals.dealNumber));
            SqlDataReader reader = command.ExecuteReader();
            bool stat = reader.HasRows;
            reader.Close();
            return stat;
        }

        private static List<WoodDeal> GetWoodDeals()
        {
            Thread.Sleep(1000);
            GraphQLRequest graphQLRequest = new GraphQLRequest();
            graphQLRequest.query = "query SearchReportWoodDeal($size: Int!, $number: Int!, $filter: Filter, $orders: [Order!]) { " +
                "   searchReportWoodDeal(filter: $filter, pageable: {number: $number, size: $size}, orders: $orders) { " +
                "       content { " +
                "           sellerName " +
                "           sellerInn " +
                "           buyerName " +
                "           buyerInn " +
                "           woodVolumeBuyer " +
                "           woodVolumeSeller " +
                "           dealDate " +
                "           dealNumber " +
                "       } " +
                "   } " +
                "}";

            graphQLRequest.operationName = "SearchReportWoodDeal";
            graphQLRequest.variables = new GraphQLRequestVariables { size = recordsOnPage, number = page };
            string str = JsonConvert.SerializeObject(graphQLRequest);
            byte[] byteArray = Encoding.UTF8.GetBytes(str);

            WebClient webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "application/json");
            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36");
            byte[] byteResult = webClient.UploadData("https://www.lesegais.ru/open-area/graphql", "POST", byteArray);
            string responseText = Encoding.UTF8.GetString(byteResult);

            GraphQLResponse json = JsonConvert.DeserializeObject<GraphQLResponse>(responseText);
            List<WoodDeal> woodDeals = json.data.searchReportWoodDeal.content;
            return woodDeals;
        }
    }

    class GraphQLRequest
    {
        public string operationName { get; set; }
        public string query { get; set; }
        public GraphQLRequestVariables variables { get; set; }
    }

    class GraphQLRequestVariables
    {
        public int size { get; set; }
        public int number { get; set; }
    }

    class GraphQLResponse
    {
        public WoodDealData data { get; set; }
    }

    public class WoodDealData
    {
        public WoodDealSearchReport searchReportWoodDeal { get; set; }
    }
    public class WoodDealSearchReport
    {
        public List<WoodDeal> content { get; set; }
    }

    public class WoodDeal
    {
        public string sellerName { get; set; }
        public string sellerInn { get; set; }
        public string buyerName { get; set; }
        public string buyerInn { get; set; }
        public float woodVolumeBuyer { get; set; }
        public float woodVolumeSeller { get; set; }
        public string dealDate { get; set; }
        public string dealNumber { get; set; }
    }
}
