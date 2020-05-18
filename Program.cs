using HtmlAgilityPack;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace MoonCake_AutoElvui
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Recherche du fichier de configuration...");
            
            if (!File.Exists("retail.txt")) {
                Console.WriteLine("Le fichier de configuration n'existe pas...");
                Console.WriteLine("Veuillez créer un fichier texte juste en dessous de ce logiciel avec le chemin vers WoW Retail !");
                End();
            }

            Console.WriteLine("Recherche du dossier d'installation de WoW Retail...");
            String retailPath = File.ReadAllText("retail.txt");

            // Recherche de wow
            if (Directory.Exists(retailPath))
            {
                // Recherche de l'existance d'Elvui
                if (!Directory.Exists(Path.Combine(retailPath, @"Interface\AddOns\ElvUI")))
                {
                    Console.WriteLine("(Er:1) Pour pouvoir être mise à jour, Elvui doit être installé...");
                    End();
                }
            }
            else {
                Console.WriteLine("(Er2:)Le chemin dans 'retail.txt' n'existe pas...");
                End();
            }

            // Extraction de la version sur le site
            Console.WriteLine("Recherche de la version de Elvui sur leur serveur...");
            string urlAddress = "https://www.tukui.org/download.php?ui=elvui";

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(new WebClient().DownloadString(urlAddress));

            HtmlNode divMaster = doc.DocumentNode.SelectSingleNode("//div[@id=\"download\"]");
            HtmlNode aDL = divMaster.SelectSingleNode("//a[starts-with(@href, '/downloads')]");

            String link = aDL.GetAttributeValue("href", null);

            string[] dirt = link.Split(new[] { "elvui-" }, StringSplitOptions.None);
            dirt = dirt[dirt.Length - 1].Split(new[] { ".zip" }, StringSplitOptions.None);
            float server_version = float.Parse(dirt[0].Replace(".", ","));
            Console.WriteLine($"Version disponible : {server_version}");
            Console.WriteLine($"Voulez vous télécharger et installer cette version ? (y/n)");
            ConsoleKeyInfo input = Console.ReadKey();

            switch (input.Key)
            {
                case ConsoleKey.Y:
                    Console.WriteLine($"\r\n");
                    break;
                case ConsoleKey.N:
                    Console.WriteLine($"\r\nLa version {server_version} ne sera pas installée.");
                    End();
                    break;
                default:
                    Console.WriteLine($"\r\nJe prends ça pour un oui");
                    break;
            }

            // Télécharger la dernière version
            Console.WriteLine($"Téléchargement de l'archive, veuillez patienter...");
            string lastVersionUrl = "https://www.tukui.org" + link;

            WebClient webClient = new WebClient();
            webClient.Headers.Add("Accept: text/html, application/xhtml+xml, */*");
            webClient.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");

            // Il faut un dossier temporaire pour extraire le zip avant de le deplacer
            Directory.CreateDirectory("Temporary_extract_folder");
            webClient.DownloadFile(new Uri(lastVersionUrl), Path.Combine("Temporary_extract_folder", "lastversion.zip"));

            Console.WriteLine($"Archive téléchargée, installation en cours, veuillez patienter...");
            if (Directory.Exists(Path.Combine("Temporary_extract_folder", "Extractor")))
            {
                Directory.Delete(Path.Combine("Temporary_extract_folder", "Extractor"), true);
            }

            Directory.CreateDirectory(Path.Combine("Temporary_extract_folder", "Extractor"));

            ZipFile.ExtractToDirectory(Path.Combine("Temporary_extract_folder", "Lastversion.zip"), Path.Combine("Temporary_extract_folder", "Extractor"));

            DirectoryInfo elvui = new DirectoryInfo(Path.Combine("Temporary_extract_folder", "Extractor") + "/ElvUI");
            DirectoryInfo elvuiWow = new DirectoryInfo(Path.Combine(retailPath, @"Interface\AddOns\ElvUI"));

            DirectoryInfo elvui_config = new DirectoryInfo(Path.Combine("Temporary_extract_folder", "Extractor") + "/ElvUI_OptionsUI");
            DirectoryInfo elvui_configWow = new DirectoryInfo(Path.Combine(retailPath, @"Interface\AddOns\ElvUI_OptionsUI"));

            CopyAll(elvui, elvuiWow);

            CopyAll(elvui_config, elvui_configWow);

            Console.WriteLine($"Installation terminée !");
            End();

        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (source.FullName.ToLower() == target.FullName.ToLower())
            {
                return;
            }

            // Vérifiez si le répertoire cible existe, si ce n'est pas le cas, créez-le.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copiez chaque fichier dans son nouveau répertoire.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copiez chaque sous-répertoire en utilisant la récursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        private static void End() {
            Console.WriteLine($"Appuyer sur n'importe quelle touche pour fermer le programme.");
            ConsoleKeyInfo input = Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
