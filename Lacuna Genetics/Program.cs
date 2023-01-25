using Newtonsoft.Json;
using System.Text;

namespace LacunaGenetics
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // await PostNewUser();
            await PostLogin();
        }

        public static async Task PostNewUser()
        {
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage();

            var objeto = new { username = "williamdistler", email = "william.distler@outlook.com", password = "L@cuna2023" };

            var body = ToRequest(objeto);

            var response = await httpClient.PostAsync("https://gene.lacuna.cc/api/users/create", body);

            Console.WriteLine("============== CRIANDO USUARIO ===================================");

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
            } else
            {
                Console.WriteLine(response);
            }

        }

        public static async Task PostLogin()
        {
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage();

            var objeto = new { username = "williamdistler", password = "L@cuna2023" };

            var body = ToRequest(objeto);

            var response = await httpClient.PostAsync("https://gene.lacuna.cc/api/users/login", body);

            Console.WriteLine("========================= LOGIN ===================================");

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
                var data = JsonConvert.DeserializeObject<dynamic>(responseContent);
                var accessToken = data.accessToken;
                var token = accessToken.ToString();
                await GetJob(token);
            }
            else
            {
                Console.WriteLine(response);
            }

        }

        public static async Task GetJob(string token)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            HttpResponseMessage response = await client.GetAsync("https://gene.lacuna.cc/api/dna/jobs");

            string responseContent = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(responseContent);
            var id = data.job.id.ToString();
            var type = data.job.type.ToString();

            Console.WriteLine("========================= PEGANDO JOB ===================================");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(responseContent);
            }
            else
            {
                Console.WriteLine(response);
            }

            switch (type)
            {
                case "DecodeStrand":
                    var strandEncoded = data.job.strandEncoded.ToString();
                    await DecodeStrand(id, token, strandEncoded);
                    break;
                case "EncodeStrand":
                    var strand = data.job.strand.ToString();
                    await EncodeStrand(id, token, strand);
                    break;
                case "CheckGene":
                    var geneEncoded = data.job.geneEncoded.ToString();
                    var strandEncodedCheckGene = data.job.strandEncoded.ToString();
                    await CheckGene(id, token, geneEncoded, strandEncodedCheckGene);
                    break;
            }

        }

        public static async Task DecodeStrand(string id, string token, string strandEncoded)
        {
            string base64String = strandEncoded;
            string decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var objeto = new { strandEncoded = decodedString };

            var body = ToRequest(objeto);

            string url = $"https://gene.lacuna.cc/api/dna/jobs/{id}/decode";

            var response = await client.PostAsync(url, body);
            string responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine("========================= DECODE STRAND ===================================");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(responseContent);
            }
            else
            {
                Console.WriteLine(response);
            }
        }

        public static async Task EncodeStrand(string id, string token, string strand)
        {
            string inputString = strand;
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);
            List<int> binaryList = new List<int>();

            foreach (byte b in inputBytes)
            {
                for (int i = 0; i < 8; i++)
                {
                    int bit = (b >> i) & 1;
                    binaryList.Add(bit);
                }
            }

            int[] binaryArray = binaryList.ToArray();
            string binaryString = string.Concat(binaryArray.Select(x => x.ToString()));


            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var request = new HttpRequestMessage();

            var objeto = new { strand = binaryString };

            var body = ToRequest(objeto);

            string url = $"https://gene.lacuna.cc/api/dna/jobs/{id}/encode";

            var response = await client.PostAsync(url, body);
            string responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine("========================= ENCODE STRAND ===================================");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(responseContent);
            }
            else
            {
                Console.WriteLine(response);
            }
        }

        public static async Task CheckGene(string id, string token, string geneEncoded, string strandEncoded)
        {
            string gene = geneEncoded;
            string dnaTemplate = strandEncoded;
            var activated = false;

            int matchCount = 0;
            for (int i = 0; i < dnaTemplate.Length - gene.Length; i++)
            {
                int tempMatchCount = 0;
                for (int j = 0; j < gene.Length; j++)
                {
                    if (gene[j] == dnaTemplate[i + j])
                    {
                        tempMatchCount++;
                    }
                }
                if (tempMatchCount > matchCount)
                {
                    matchCount = tempMatchCount;
                }
            }
            double matchPercentage = (double)matchCount / gene.Length * 100;
            if (matchPercentage > 50)
            {
                activated = true;
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var objeto = new { isActivated = activated };

            var body = ToRequest(objeto);

            string url = $"https://gene.lacuna.cc/api/dna/jobs/{id}/gene";

            var response = await client.PostAsync(url, body);
            string responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine("========================= CHECK GENE ===================================");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(responseContent);
            }
            else
            {
                Console.WriteLine(response);
            }
        }

        private static StringContent ToRequest(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var data = new StringContent(json, System.Text.Encoding.UTF8, mediaType: "application/json");

            return data;
        }
    }
}