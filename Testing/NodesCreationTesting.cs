using System.Collections.Concurrent;
using ARMO_Test1;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing
{
    [TestClass]
    public class NodesCreationTesting
    {
        [TestMethod]
        public void NodesAddingTest1()
        {
            typeof(SearchFiles).GetProperty(nameof(SearchFiles.FilesQueue))
                ?.SetValue(new ConcurrentQueue<string>(), 
                    new ConcurrentQueue<string>());
            SearchFiles.FilesQueue.Enqueue("C:\\Program Files\\Adobe\\Adobe Photoshop CC 2017\\ACE.dll");


            var expectedTree = new TreeView();
            expectedTree.Nodes.Add("C:\\", "C:\\")
                .Nodes.Add("Program Files", "Program Files")
                .Nodes.Add("Adobe", "Adobe")
                .Nodes.Add("Adobe Photoshop CC 2017", "Adobe Photoshop CC 2017")
                .Nodes.Add("ACE.dll", "ACE.dll");

            var tree = new TreeView();
            NodeOperations.AddNodeFromPath(tree);

            Assert.AreEqual(expectedTree.Nodes[0].Text, tree.Nodes[0].Text);
            Assert.AreEqual(expectedTree.Nodes[0].Nodes[0].Text, tree.Nodes[0].Nodes[0].Text);
            Assert.AreEqual(expectedTree.Nodes[0].Nodes[0].Nodes[0].Text, tree.Nodes[0].Nodes[0].Nodes[0].Text);
            Assert.AreEqual(expectedTree.Nodes[0].Nodes[0].Nodes[0].Nodes[0].Text, 
                tree.Nodes[0].Nodes[0].Nodes[0].Nodes[0].Text);
            Assert.AreEqual(expectedTree.Nodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0].Text, 
                tree.Nodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0].Text);
        }
    }
}
