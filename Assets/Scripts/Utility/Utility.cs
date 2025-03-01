// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using System;
using UnityEngine;
using Puppet;
using System.IO;

public class Utility
{
	public static class Convert{
		/// <summary>
		/// Định dạng kiểu tiền trong game (Eg: 10.000.000 Chip)
		/// </summary>
		public static string ConvertShortcutMoney(System.Object chip)
		{
			long money = 0;
			if (long.TryParse(chip.ToString(), out money) == false || money == 0)
				return "0";
			if (money > 1000 && money < 1000000)
			  return string.Format("{0:#,##}K", Mathf.Round(money / 1000));
			else if (money >= 1000000)
			  return string.Format("{0:#,##}M", Mathf.Round(money / 1000000));
//			if (money > 1000)
//				return string.Format("{0:#,##}K", Mathf.Round(money / 1000));
			
			return string.Format("{0:#,##}", money);
		}
		public static string[] ConvertMoneyAndShortCut(System.Object chip)
		{
			string[] moneyArray = new string[2];
			long money = 0;
			if (long.TryParse (chip.ToString (), out money) == false || money == 0) 
			{
				moneyArray[0] = "0";
				moneyArray[1] = "";
			}
			if (money < 1000000) {
				if (money < 1000) {
					moneyArray [0] = money.ToString ();
					moneyArray [1] = "";
				} else {
					string myString8 = String.Format("{0, 0:f2}", money / 1000f);
					moneyArray [0] = myString8;
					moneyArray [1] = "K";
				}
				return moneyArray;
			} else {
				string myString8 = String.Format("{0, 0:f2}", money / 1000000f);
				moneyArray [0] = myString8;
				moneyArray [1] = "M";
				return moneyArray;
			}
		}
	}
    
    public static class ReadData
    {
        public static string ReadDataWithKey(string filePath, string separation, string key)
        {
            if (File.Exists(filePath))
            {
                foreach (string s in File.ReadAllLines(filePath))
                {
                    string[] data = s.Split(separation.ToCharArray(), StringSplitOptions.None);
                    if (data.Length > 1 && data[0] == key)
                        return data[1];
                }
            }
            return null;
        }

        public static string GetTrackId()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "inject.txt");
            return ReadDataWithKey(path, "=", "PTE_TRACK_ID");
        }
    }
}


