using System;
using System.IO;
using System.Security.Cryptography;

namespace encryptCS
{
    class Program
    {
        static Aes KeyGen(string fileName)
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
            {
                var aes = Aes.Create();
                fileStream.Write(aes.Key, 0, aes.IV.Length);
                fileStream.Write(aes.IV, 0, aes.IV.Length);
                return aes;
            }
        }


        static Aes KeyLoad(string fileName)
        {
            if (File.Exists(fileName))
            {
                using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
                {
                    var aes = Aes.Create();
                    if (fileStream.Length == 32)
                    {
                        byte[] key = new byte[32];
                        byte[] iv = new byte[16];
                        fileStream.Read(key, 0, key.Length);
                        fileStream.Read(iv, 0, iv.Length);


                        aes.Key = key;
                        aes.IV = iv;

                        return aes;
                    }
                }
            }
            return KeyGen(fileName);
        }


        static void Encrypt(string fileName, Aes aes)
        {
            byte[] plainBytes;
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
            {
                plainBytes = new byte[fileStream.Length];
                using (var cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Read))
                {
                    fileStream.Read(plainBytes, 0, (int) fileStream.Length);
                }
            }
            File.WriteAllText(fileName, string.Empty);
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
            {
                using (CryptoStream cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                }
            }
        }



        static void Decrypt(string fileName, Aes aes)
        {
            byte[] encryptedBytes;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
            {
                encryptedBytes = new byte[fileStream.Length];
                using (var cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read))
                {
                    fileStream.Read(encryptedBytes, 0, (int) fileStream.Length);
                }
            }
            File.WriteAllText(fileName, string.Empty);
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
            {
                using (CryptoStream cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                }
            }
        }


        static void Main(string[] args)
        {
            Aes aes = KeyLoad("Key.txt");
            string fileName;
            string option = "";
            while (true)
            {
                Console.WriteLine("what command do you want to do?");
                Console.WriteLine("e) encrypt");
                Console.WriteLine("d) decrypt");
                Console.WriteLine("g) generate key");
                Console.WriteLine("c) close");
                option = Console.ReadLine();

                switch (option)
                {
                    case "e":
                        Console.WriteLine("What file do you want to encrypt?");
                        fileName = Console.ReadLine();
                        Encrypt(fileName, aes);
                        break;
                    case "d":
                        Console.WriteLine("What file do you want to Decrypt");
                        fileName = Console.ReadLine();
                        Decrypt(fileName, aes);
                        break;
                    case "g":
                        KeyGen("key.txt");
                        Console.WriteLine("key was generated");
                        break;
                    case "c":
                        break;
                }
            }
        }
    }
}
