using ARMO_Test1;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing
{
    [TestClass]
    public class ReadFileTest
    {
        [TestMethod]
        public void ReadFile()
        {
            var result = new ReadWriteSaveInputs();
            Assert.IsNotNull(result);
        }
    }
}
