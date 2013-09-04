using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Diagnostics;


namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //List<string> strList = new List<string> { "a", "b", "c" };

            //List<string> newList = strList.Select(_str => _str += "111").ToList<string>();

            //foreach (string s in strList)
            //{
            //    Debug.WriteLine(s);
            //}
            //foreach (string s in newList)
            //{
            //    Debug.WriteLine(s);
            //}


            List<int> intList = new List<int> { 1, 2, 3 };
            List<int> newIntList = intList.Select(_i => _i=_i+1).ToList<int>();

            foreach (int i in intList)
            {
                Debug.WriteLine(i.ToString());
            }

            foreach (int i in newIntList)
            {
                Debug.WriteLine(i.ToString());
            
            }
        }
    }
}
