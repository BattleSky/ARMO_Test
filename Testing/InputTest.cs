using System;
using System.IO;
using System.Threading.Tasks;
using ARMO_Test1;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing
{
    [TestClass]
    public class InputTest
    {
        [TestMethod]
        public async Task IncorrectDirectoryInput()
        {
            await Assert.ThrowsExceptionAsync<DirectoryNotFoundException>(async () => await SearchFiles.SearchByRegex("1", "12348-32104283-05-14857918-1"));
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await SearchFiles.SearchByRegex("1", ""));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await SearchFiles.SearchByRegex("1", null));
            Assert.AreEqual(true, SearchFiles.SearchCancelledOrDone);
        }

        [TestMethod]
        public async Task IncorrectRegexInput()
        {
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () => await SearchFiles.SearchByRegex("}{!}@#{!}$!{}!{%%!)%(!_)()$(*!$)(!@#_)!@#", @"C:\\"));
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () => await SearchFiles.SearchByRegex(null, @"C:\\"));
            var cancelledSearch = SearchFiles.SearchCancelledOrDone;
            Assert.AreEqual(true, cancelledSearch);
            Assert.AreEqual(true, ActionForm.CancellationToken.IsCancellationRequested);
            ActionForm.CancellationToken.Cancel();
        }
    }
}
