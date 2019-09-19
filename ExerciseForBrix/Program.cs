using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ExerciseForBrix
{
    //Created by Pini Rotblat
    class Program
    {
        static void Main(string[] args)
        {
            //Exercise 1
            //make Person and Super

            //Create 10 people shopping
            List<Person> persons = new List<Person>();
            CreatePerson(persons);

            //Create Super and Cashiers
            CreateSuperAndCashiers(persons);

            //wait to finish first Exercise
            System.Threading.Thread.Sleep(15000);

            //Exercise 2
            FindText();
            // Suspend the screen.  
            System.Console.ReadLine();
        }

        //Exercise 1
        static readonly object _locker = new object();
        //make Person and Super
        private static async void CreateSuperAndCashiers(List<Person> persons)
        {
            //Create super with 5 cashir
            Super super = new Super(5);
            super.OrderProcessing += (o, e) =>
            {
                if (persons.Count > 0)
                {
                    Person person = persons.Where(p => p.GetService == false && p.LineNo > 0).OrderBy(p => p.LineNo).FirstOrDefault();
                    if (person != null)
                    {
                    //lock for conflict
                    Monitor.Enter(_locker);
                        try
                        {
                            if (!person.GetService)
                            {
                                Console.WriteLine($"Person {person.PersonName} get service from cashier position: {e.Position} in time: {e.Time} second.");
                                person.GetService = true;
                            }
                        }
                        finally
                        {
                            Monitor.Exit(_locker);
                        }
                    }
                }
            };

            //Generate the Cashiers in line for persons services
            await super.OrderCashierAsync();
            
        }

        public static Task CreatePerson(List<Person> persons)
        {
            return Task.Run(() =>
            {
                for (int i = 1; i <= 10; i++)
                {
                    System.Threading.Thread.Sleep(1000); //wait one second for each person
                    Person person = new Person($"pini{i}");
                    person.LineNo = i;
                    persons.Add(person);
                    Console.WriteLine($"Person {person.PersonName} get in line no. {person.LineNo}.");
                }
            });
        }

        //Exercise 2
        private static void FindText()
        {
            string inputData;
            do
            {
                Console.WriteLine("Enter 5 characters");
                inputData = Console.ReadLine();
                if (inputData.Length != 5)
                    Console.WriteLine("Enter 5 characters");

            } while (inputData.Length != 5);

            int counter = 0;
            string line;
            int total = 0;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            CharacterArr inputSort = new CharacterArr(inputData); //sort the input data
                                                                  // Read the file  
            System.IO.StreamReader file =
                new System.IO.StreamReader(@"C:\Pini\Dev\c#\brix\ExerciseForBrix\ExerciseForBrix\Data\data.txt");
            while ((line = file.ReadLine()) != null)
            {
                CharacterArr lineSort = new CharacterArr(line);
                if (inputSort.Input == lineSort.Input)
                {
                    total++;
                }
                counter++;
            }

            file.Close();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"total execute time: { elapsedMs } Ms");
            Console.WriteLine("There were {0} lines \nThe input was {1} \nThe cases of identity was {2}", counter, inputData, total);

        }

    }

    //*************************************
    //For Exercise 1
    public class Person
    {
        public string PersonName { get; private set; }
        public int LineNo { get; set; } = 0;
        public bool GetService { get; set; } = false;
        public Person(string personName)
        {
            PersonName = personName;
        }
    }

    public class Cashier
    {
        public int Position { get; private set; }
        public int Time { get; set; }
        public Cashier(int position)
        {
            Position = position;
        }
    }

    public class CashierEventArgs : EventArgs
    {
        public int Position { get; private set; }
        public int Time { get; private set; }
        public CashierEventArgs(int position, int time)
        {
            Position = position;
            Time = time;
        }
    }

    public class Super
    {
        public event EventHandler<CashierEventArgs> OrderProcessing;
        private int _numberCashier;
        public Super(int numberCashier)
        {
            _numberCashier = numberCashier;
        }

        private Random rnd = new Random();

        public void OrderCashier()
        {
            //Create 5 Cashires in Super
            List<Cashier> cashiers = new List<Cashier>();
            for (int i = 1; i <= _numberCashier; i++)
            {
                Cashier cashier = new Cashier(i);
                cashier.Time = rnd.Next(1, 6);
                cashiers.Add(cashier);
            }

            //Run in loop in paralel over all 5 cashiers for 10 sircles
            int j = 1;
            do
            {
                Parallel.ForEach<Cashier>(cashiers, (cashier) =>
                {
                    WaitCashier(cashier);
                });
                j++;
            } while (j < 10);

        }

        public Task OrderCashierAsync()
        {
            return Task.Run(() =>
            {
                OrderCashier();
            });
        }

        private void WaitCashier(Cashier cashier)
        {
            System.Threading.Thread.Sleep(cashier.Time * 1000); //sleep
            OrderProcessing?.Invoke(this, new CashierEventArgs(cashier.Position, cashier.Time));
            //After cashier finish, he get new time
            cashier.Time = rnd.Next(1, 6);
        }

    }

    //***********************************

    //***********************************
    //For Exercise 2
    public class CharacterArr
    {
        private string _input;
        public CharacterArr(string input)
        {
            _input = input;
            sortInput();
        }
        private void sortInput()
        {
            char[] characters = _input.ToCharArray();
            Array.Sort(characters);
            _input = new string(characters);
        }
        public string Input
        {
            get
            {
                return _input;
            }
        }
    }
    //********************************
}
