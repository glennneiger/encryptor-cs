using System;
using System.IO;
using System.Security.Cryptography;

namespace encryptCS
{
    class Program
    {
        static Aes KeyGen(string fileName)
        {
            //creating the key file
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
            {
                //creating key
                var aes = Aes.Create();
                //wriing key into the file
                fileStream.Write(aes.Key, 0, aes.Key.Length);
                fileStream.Write(aes.IV, 0, aes.IV.Length);
                //returning key
                return aes;
            }
        }


        static Aes KeyLoad(string fileName)
        {
            //only if the file exists
            if (File.Exists(fileName))
            {
                //openin a file
                using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
                {
                    //creating key
                    var aes = Aes.Create();
                    //if the file is in key size
                    if (fileStream.Length == 48)
                    {
                        //making the key parts
                        byte[] key = new byte[32];
                        byte[] iv = new byte[16];
                        //reading key parts into the key parts
                        fileStream.Read(key, 0, key.Length);
                        fileStream.Read(iv, 0, iv.Length);

                        //inserting key parts into the key
                        aes.Key = key;
                        aes.IV = iv;
                        //returning key
                        return aes;
                    }
                }
            }
            //if file does not exist or not in key size than generate key
            return KeyGen(fileName);
        }


        static void Encrypt(string fileName, ICryptoTransform cryptoTransform)
        {
            byte[] plainBytes;
            //opening the targeted file
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
            {
                plainBytes = new byte[fileStream.Length];
                //creating a temp file
                using (FileStream fileStream2 = new FileStream("temp.txt", FileMode.Create))
                {
                    //opening a cypto stream on the targeted file stream using the encryptor
                    {
                        using(CryptoStream cryptoStream = new CryptoStream(fileStream2, cryptoTransform, CryptoStreamMode.Write))
                        {
                            int fileLength = 0;
                            while(true)
                            {
                                //if file Length - what we did is smaller than 1024
                                if((int) fileStream.Length - fileLength < 1024)
                                {
                                    //but only if its bigger than 0
                                    if((int) fileStream.Length - fileLength > 0)
                                    {
                                        //reading file to plaing bytes
                                        fileStream.Read(plainBytes, 0, (int) fileStream.Length - fileLength);
                                        //wrting plain bytes to temp
                                        cryptoStream.Write(plainBytes, 0, (int) fileStream.Length - fileLength);
                                    }
                                    break;
                                }
                                //reading file to plain bytes
                                fileStream.Read(plainBytes, 0, 1024);
                                //adding to file length
                                fileLength = fileLength + 1024;
                                //writing plain bytes to temp
                                cryptoStream.Write(plainBytes, 0, 1024);
                            }
                        }
                    }                                                                                                                                          
                }
            }
            //moving temp.txt to the targeted file
            File.Copy("temp.txt", fileName, true);
            File.Delete("temp.txt");
        }

        static void Decrypt(string fileName, Aes aes)
        {
            byte[] encryptedBytes;
            //opening the targeted file
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
            {
                encryptedBytes = new byte[fileStream.Length];
                //creating decryptor
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                //creating a temp file
                using (FileStream fileStream2 = new FileStream("temp.txt", FileMode.Create))
                {
                    //opening a cypto stream on the targeted file stream using the decryptor
                    using (CryptoStream cryptoStream = new CryptoStream(fileStream2, decryptor, CryptoStreamMode.Write))
                    {
                        int fileLength = 0;
                        while (true)
                        {
                            //if file length  - what we did < then 1024
                            if ((int)fileStream.Length - fileLength < 1024)
                            {
                                //but only if its bigger than 0
                                if ((int)fileStream.Length - fileLength > 0)
                                {
                                    //reading file to encrypted bytes
                                    fileStream.Read(encryptedBytes, 0, (int)fileStream.Length - fileLength);
                                    //writing encrypted bytes into the temp file
                                    cryptoStream.Write(encryptedBytes, 0, (int)fileStream.Length - fileLength);
                                }
                                break;
                            }
                            //read to encrypted bytes
                            fileStream.Read(encryptedBytes, 0, 1024);
                            //add 1024 to what we did
                            fileLength = fileLength + 1024;
                            //write to temp
                            cryptoStream.Write(encryptedBytes, 0, 1024);
                        }
                    }
                }
            }
            //moving temp to the targeted file
            File.Copy("temp.txt", fileName, true);
            File.Delete("temp.txt");
        }


        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            //loading key from key.txt
            Aes aes = KeyLoad("Key.key");
            string pathName;
            string option = "";
            string yesOrNo;
            //loop
            while (true)
            {
                //making gui
                Console.WriteLine("what command do you want to do?");
                Console.WriteLine("e) encrypt");
                Console.WriteLine("d) decrypt");
                Console.WriteLine("g) generate key");
                Console.WriteLine("c) close");
                option = Console.ReadLine();

                //making cases for option
                switch (option)
                {
                    case "e":
                        bool keyEncrypted = false;
                        Console.WriteLine("what do you want to encrypt");
                        pathName = Console.ReadLine();
                        //creating encryptor
                        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                        if (File.Exists(pathName))
                        {
                            //encrypting file
                            Encrypt(pathName, encryptor);
                            if (pathName == "key.key")
                            {
                                Decrypt("key.key", aes);
                            }
                        }
                        else if(Directory.Exists(pathName))
                        {
                            //taking files in folder
                            string[] folder = Directory.GetFiles(pathName, "*", SearchOption.AllDirectories);
                            //for each file in folder
                            foreach (string file in folder)
                            {
                                //encrypting file with the key
                                Encrypt(file, encryptor);
                                //if the file is the key file than decrypt key
                                if (file == "key.key")
                                {
                                    keyEncrypted = true;
                                }
                            }
                            if (keyEncrypted == true)
                            {
                                Decrypt("key.key", aes);
                            }
                        }
                        break;
                    case "d":
                        Console.WriteLine("what do you want to decrypt.");
                            
                        pathName = Console.ReadLine();
                        if(File.Exists(pathName))
                        {
                            //decrypting file
                            Decrypt(pathName, aes);
                            break;
                        }
                        else if(Directory.Exists(pathName))
                        {
                            string[] folder = Directory.GetFiles(pathName, "*", SearchOption.AllDirectories);
                            foreach (string file in folder)
                            {
                                Decrypt(file, aes);
                            }
                        }
                        break;
                    case "g":
                        //generating a new key
                        aes = KeyGen("key.key");
                        Console.WriteLine("key was generated");
                        break;
                    case "c":
                        //closing program
                        Environment.Exit(1);
                        break;
                }
            }
        }
    }
}
