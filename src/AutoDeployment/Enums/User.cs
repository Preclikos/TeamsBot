using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace AutoDeployment.Enums
{
    public class User
    {
        private static readonly Dictionary<int, ChannelAccount> keyNameAccount = new Dictionary<int, ChannelAccount>();

        static User()
        {
            keyNameAccount.Add((int)Name.Husak, new ChannelAccount() { Id = "29:1nCs_QjzTvPhxRXsm4ybwHPA_BqSSZGxpsSKu4Y2yhc8t5WBFE_Sh1fQgRUACY7jmloq_EHZbubFPLWt7VPt9xQ", Name = "Jan Husák" });
            keyNameAccount.Add((int)Name.Gabela, new ChannelAccount() { Id = "29:1mgjmuSINsXdeFRdhHTVoYKQEeFpEhjbpciA_illntb450LBIBj0kXUpe_Zs2ySNQBihftzWDbn5r7gT_G-cPwA", Name = "Ján Gábela" });
            keyNameAccount.Add((int)Name.Patrik, new ChannelAccount() { Id = "29:15vxnq1Y4L6NKcklQLHQF-tYGh7E4E-1tggB4iOqeaSZyYxhE_mSVj2Q5JH81EyBj5ZOmwOSHjrhyZD401T9SlQ", Name = "Pavel Berger" });
            keyNameAccount.Add((int)Name.Simera, new ChannelAccount() { Id = "29:1T54I5tm1uVh4yugOoso3Fm9YX4IAVSL9kx5nPrko1bxbVTAhN6cw32yOspdfYTULebjsgk_7nW8ax1ct4O-0Tg", Name = "Daniel Šimera" });
            keyNameAccount.Add((int)Name.Dulak, new ChannelAccount() { Id = "29:1STiCGnowmvl4J3AIAKaQ3Rdn4UqIphXLNSZ9Q-bkTuZTTgA4nPCUpo1xlTmqoEb2yEUVLjRgZl1j7oi-1TNhmg", Name = "Radek Ďulák" });
            keyNameAccount.Add((int)Name.Vlk, new ChannelAccount() { Id = "29:1tWQbgEjzrTHC8RtAYbagrlN1clLWwkFcU9WEXPeopUIgHKefFk3jl9J9ip6L1QOLi5XVCo_od13EzLu5jxwT-w", Name = "Michal Vlk" });
            keyNameAccount.Add((int)Name.Holubek, new ChannelAccount() { Id = "RDZEaMB9KpbJc4eSSpI48uXE85i7mbDV34it4heUA0zyqhZx6GFXYEHKM8KAbLGn90UPyDaR-4fN9GY6zA", Name = "Miloslav Holúbek" });
            keyNameAccount.Add((int)Name.Masaryk, new ChannelAccount() { Id = "29:1NaEnlR8lFKY-Vek0jqRHnkksMZy7wXfCnhJOX8BvkHowGtqHJR990vMl-iKd7qPTrSHW5wlUqhV42x0OIB5meA", Name = "Matúš Masaryk" });
        }

        public enum Name { Husak = 1 , Gabela = 2, Patrik = 3, Simera = 4, Dulak = 5, Vlk = 6, Holubek = 7, Masaryk = 8};

        public static ChannelAccount GetId(Name names)
        {
            return keyNameAccount.GetValueOrDefault((int)names, null);
        }

        private static RNGCryptoServiceProvider rngCsp { get; set; }

        public static Name GetRandom()
        {
            rngCsp = new RNGCryptoServiceProvider();

            Array values = Enum.GetValues(typeof(Name));

            const int totalRolls = 25000;
            int[] results = new int[values.Length];

            // Roll the dice 25000 times and display
            // the results to the console.
            for (int x = 0; x < totalRolls; x++)
            {
                byte roll = RollDice((byte)results.Length);
                results[roll - 1]++;
            }

            var maxValue = results.Max();
            var maxIndex = results.ToList().IndexOf(maxValue);

            rngCsp.Dispose();
            return (Name)values.GetValue(maxIndex);
        }

        // This method simulates a roll of the dice. The input parameter is the
        // number of sides of the dice.

        public static byte RollDice(byte numberSides)
        {
            if (numberSides <= 0)
                throw new ArgumentOutOfRangeException("numberSides");

            // Create a byte array to hold the random value.
            byte[] randomNumber = new byte[1];
            do
            {
                // Fill the array with a random value.
                rngCsp.GetBytes(randomNumber);
            }
            while (!IsFairRoll(randomNumber[0], numberSides));
            // Return the random number mod the number
            // of sides.  The possible values are zero-
            // based, so we add one.
            return (byte)((randomNumber[0] % numberSides) + 1);
        }

        private static bool IsFairRoll(byte roll, byte numSides)
        {
            // There are MaxValue / numSides full sets of numbers that can come up
            // in a single byte.  For instance, if we have a 6 sided die, there are
            // 42 full sets of 1-6 that come up.  The 43rd set is incomplete.
            int fullSetsOfValues = Byte.MaxValue / numSides;

            // If the roll is within this range of fair values, then we let it continue.
            // In the 6 sided die case, a roll between 0 and 251 is allowed.  (We use
            // < rather than <= since the = portion allows through an extra 0 value).
            // 252 through 255 would provide an extra 0, 1, 2, 3 so they are not fair
            // to use.
            return roll < numSides * fullSetsOfValues;
        }
    }
}
