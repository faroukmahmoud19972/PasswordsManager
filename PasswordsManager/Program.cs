using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace train
{
    internal class Program
    {
        private static readonly Dictionary<string, Dictionary<string, string>> _dic = new Dictionary<string, Dictionary<string, string>>();

        private delegate void Operation();

        static void Main(String[] args)
        {
            Read();

            var operations = new Dictionary<int, Operation> {
                { 1, ListAllPasswords },
                { 2, AddPassword },
                { 3, EditPassword },
                { 4, DeletePassword }
            };

            while (true)
            {
                int op = GetOperation();

                if (operations.ContainsKey(op))
                    operations[op]();
                else
                    Console.WriteLine("Invalid operation!\n");
            }
        }

        private static int GetOperation()
        {
            Console.WriteLine("Operations");
            Console.WriteLine("1.List all passwords\n2.Add a password\n3.Edit a password\n4.Delete a password\n");
            Console.Write("Enter the number of the operation: ");
            int op = int.Parse(Console.ReadLine());

            return op;
        }

        private static void ListAllPasswords()
        {
            int i = 1;
            foreach (var site in _dic)
            {
                Console.WriteLine($"{i++}.{site.Key}");

                foreach (var account in site.Value)
                    Console.WriteLine($"    {account.Key}:{account.Value}");
            }
            Console.WriteLine();
        }

        private static string[] GetInfo(int sourceOperation)
        {
            string[] info = new string[3];

            Console.Write("Site: ");
            info[0] = Console.ReadLine();
            Console.Write("Username: ");
            info[1] = Console.ReadLine();

            if (sourceOperation == 2)
            {
                Console.Write("Password: ");
                info[2] = Console.ReadLine();
            }
            else if (sourceOperation == 3)
            {
                Console.Write("New password: ");
                info[2] = Console.ReadLine();
            }

            info[0] = info[0].ToLower();

            return info;
        }

        private static bool Exists(string site, string username)
        {
            if (!_dic.ContainsKey(site))
            {
                Console.WriteLine("There is no such site.\n");
                return false;
            }
            else if (!_dic[site].ContainsKey(username))
            {
                Console.WriteLine("There is no such username.\n");
                return false;
            }

            return true;
        }

        private static void AddPassword()
        {
            string site, username, password;

            string[] info = GetInfo(2);
            site = info[0];
            username = info[1];
            password = info[2];

            if (_dic.ContainsKey(site) && _dic[site].ContainsKey(username))
            {
                Console.Write("This user name is already used for this site, do you wish to edit it? [Yes/No]: ");
                string tmp = Console.ReadLine();

                if (string.Equals(tmp, "yes", StringComparison.OrdinalIgnoreCase))
                    EditFromAdd(site, username, password);
            }
            else
            {
                if (_dic.ContainsKey(site))
                    _dic[site].Add(username, password);
                else
                    _dic.Add(site, new Dictionary<string, string> { { username, password } });

                Console.WriteLine("Successfully added!\n");
            }

            Save();
        }

        private static void EditPassword()
        {
            string site, username, password;

            Console.WriteLine("Enter the data to be edited");
            string[] info = GetInfo(3);
            site = info[0];
            username = info[1];
            password = info[2];

            if (!Exists(site, username))
                return;

            _dic[site][username] = password;
            Console.WriteLine("Successfully edited!\n");

            Save();
        }

        private static void EditFromAdd(string site, string username, string password)
        {
            _dic[site][username] = password;
            Console.WriteLine("Successfully edited!\n");
        }

        private static void DeletePassword()
        {
            string site, username;

            Console.WriteLine("Enter the data to be edited");
            string[] info = GetInfo(4);
            site = info[0];
            username = info[1];

            if (!Exists(site, username))
                return;

            _dic[site].Remove(username);
            if (_dic[site].Count == 0)
                _dic.Remove(site);

            Console.WriteLine("Successfully deleted!\n");

            Save();
        }

        private static void Read()
        {
            string data = File.ReadAllText("Data.txt");
            foreach (var line in data.Split(Environment.NewLine))
            {
                if (string.IsNullOrEmpty(line)) continue;

                int siteSplitter = line.IndexOf(':');
                int usernameSplitter = line.IndexOf("=");

                string site = line.Substring(0, siteSplitter);
                string username = line.Substring(siteSplitter + 1, usernameSplitter - siteSplitter - 1);
                string password = line.Substring(usernameSplitter + 1);

                if (_dic.ContainsKey(site))
                    _dic[site].Add(username, password);
                else
                    _dic.Add(site, new Dictionary<string, string> { { username, password } });
            }
        }

        private static void Save()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in _dic)
                foreach (var item in _dic[entry.Key])
                    sb.AppendLine($"{entry.Key}:{item.Key}={item.Value}");

            File.WriteAllText("Data.txt", sb.ToString());
        }
    }
}