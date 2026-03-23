using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Carrot_QA_test
{
    internal class SimulationEngine
    {
        private readonly Random random = new Random();
        private int sequence = 0;
        private const int MaxDevices = 40;

        public void Reset()
        {
            sequence = 0;
        }

        public void Tick(ObservableCollection<Taginfo> tagColl, Dictionary<string, Taginfo> tagList)
        {
            if (tagColl == null || tagList == null)
            {
                return;
            }

            if (tagColl.Count < 8 || random.Next(100) < 70)
            {
                var tag = BuildTag();
                tagList[tag.TagMac] = tag;
                tagColl.Add(tag);
            }
            else if (tagColl.Count > 0)
            {
                int idx = random.Next(tagColl.Count);
                Taginfo tag = tagColl[idx];
                RandomizeResult(tag);
            }

            while (tagColl.Count > MaxDevices)
            {
                Taginfo oldest = tagColl.OrderBy(x => x.updateTime).FirstOrDefault();
                if (oldest == null)
                {
                    break;
                }

                tagColl.Remove(oldest);
                tagList.Remove(oldest.TagMac);
            }
        }

        private Taginfo BuildTag()
        {
            sequence++;
            string macBody = sequence.ToString("X12");
            string mac = $"{macBody.Substring(0, 2)}:{macBody.Substring(2, 2)}:{macBody.Substring(4, 2)}:{macBody.Substring(6, 2)}:{macBody.Substring(8, 2)}:{macBody.Substring(10, 2)}";
            string imei = RandomDecimalDigits(15);
            string iccId = RandomDecimalDigits(19);
            string bleUuidSuffix = RandomHexDigits(12);

            var tag = new Taginfo
            {
                CarrotPlugFlag = true,
                TagName = "SIM_TAG",
                TagMac = mac,
                TagIMEI = imei,
                TagIccID = iccId,
                TagBleID = "4C520000-E25D-11EB-BA80-" + bleUuidSuffix,
                TagVersion = "v1.0",
                TagVersionNumber = 0,
                dbString = "SIM",
                passFlagUpdate = false
            };

            RandomizeResult(tag);
            return tag;
        }

        private void RandomizeResult(Taginfo tag)
        {
            bool isPass = random.Next(100) < 80;
            int rssi = random.Next(-90, -39);

            tag.TagRssi = (short)rssi;
            tag.passFlag = isPass ? "OK" : "NG";
            tag.dbString = "SIM";
            tag.TagFlagString = isPass ? "SIM PASS" : "SIM FAIL";
            tag.updateTime = DateTime.Now;
            tag.passFlagUpdate = false;
        }

        private string RandomDecimalDigits(int length)
        {
            if (length <= 0)
            {
                return string.Empty;
            }

            char[] digits = new char[length];
            digits[0] = (char)('1' + random.Next(9));
            for (int i = 1; i < length; i++)
            {
                digits[i] = (char)('0' + random.Next(10));
            }

            return new string(digits);
        }

        private string RandomHexDigits(int length)
        {
            const string hexChars = "0123456789ABCDEF";
            if (length <= 0)
            {
                return string.Empty;
            }

            char[] hex = new char[length];
            for (int i = 0; i < length; i++)
            {
                hex[i] = hexChars[random.Next(hexChars.Length)];
            }

            return new string(hex);
        }
    }
}
