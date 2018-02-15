using NUnit.Framework;
using NSubstitute;

using System.IO;

namespace SocialPoint.IO
{
    [TestFixture]
    [Category("SocialPoint.IO")]
    public sealed class FileUtilsTests
    {
        private const string TestDirectory = "io_test_directory";
        private const string TestFile = "io_test_file";

        private const string TestGeneric = "io_test_generic";
        private const string TestURL = "https://socialpoint.atlassian.net/wiki/display/MT";

        [SetUp]
        public void SetUp()
        {
            //Clean all possible remanent files from previous tests
            ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            //Clean all possible remanent files from previous tests
            ClearAll();
        }

        private void ClearAll()
        {
            try
            {
                //TODO: Use the same FileUtils functions that we want to test or use System.IO functions?
                FileUtils.DeleteDirectory(TestDirectory);
                FileUtils.DeleteDirectory(TestGeneric);
                FileUtils.DeleteFile(TestFile);
                FileUtils.DeleteFile(TestGeneric);
            }
            catch
            {
                //
            }
        }

        [Test]
        public void Create_File()
        {
            string path = TestFile;
            if(FileUtils.ExistsFile(path))
            {
                FileUtils.DeleteFile(path);
            }
            FileUtils.CreateFile(path);
            if(!FileUtils.ExistsFile(path))
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Create_Directory()
        {
            string path = TestDirectory;
            if(FileUtils.ExistsDirectory(path))
            {
                FileUtils.DeleteDirectory(path);
            }
            FileUtils.CreateDirectory(path);
            if(!FileUtils.ExistsDirectory(path))
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Create_File_Duplicated()
        {
            string path = TestFile;
            try
            {
                if(!FileUtils.ExistsFile(path))
                {
                    FileUtils.CreateFile(path);
                }
                FileUtils.CreateFile(path);
            }
            catch
            {
                //Exceptions expected upon attempt to create existent file
                if(!FileUtils.ExistsFile(path))
                {
                    Assert.Fail();
                }
                else
                {
                    Assert.Pass();
                }
            }
            //Fail if no exception was throw
            Assert.Fail();
        }

        [Test]
        public void Create_File_Bad_Path()
        {
            bool emptyPathFailed = false;
            bool invalidPathFailed = false;

            string path = string.Empty;
            try
            {
                FileUtils.CreateFile(path);
            }
            catch
            {
                emptyPathFailed = true;
            }

            path += Path.GetInvalidFileNameChars()[0];//Get any invalid char for file name
            try
            {
                FileUtils.CreateFile(path);
            }
            catch
            {
                invalidPathFailed = true;
            }

            if(!emptyPathFailed || !invalidPathFailed)
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Create_Directory_Bad_Path()
        {
            bool emptyPathFailed = false;
            bool invalidPathFailed = false;

            string path = string.Empty;
            try
            {
                FileUtils.CreateDirectory(path);
            }
            catch
            {
                emptyPathFailed = true;
            }

            path += Path.GetInvalidPathChars()[0];//Get any invalid char for path
            try
            {
                FileUtils.CreateDirectory(path);
            }
            catch
            {
                invalidPathFailed = true;
            }

            if(!emptyPathFailed || !invalidPathFailed)
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Delete_File()
        {
            string path = TestFile;
            if(!FileUtils.ExistsFile(path))
            {
                FileUtils.CreateFile(path);
            }
            FileUtils.DeleteFile(path);
            if(FileUtils.ExistsFile(path))
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Delete_Directory()
        {
            string path = TestDirectory;
            if(!FileUtils.ExistsDirectory(path))
            {
                FileUtils.CreateDirectory(path);
            }
            FileUtils.DeleteDirectory(path);
            if(FileUtils.ExistsDirectory(path))
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Write_And_Read_Byte_File()
        {
            string path = TestFile;
            if(!FileUtils.ExistsFile(path))
            {
                FileUtils.CreateFile(path);
            }
            byte[] write = { 10, 20, 30, 40, 50, 60, 70, 80, 90 };
            try
            {
                FileUtils.WriteAllBytes(path, write);
                byte[] read = FileUtils.ReadAllBytes(path);
                for(int i = 0; i < write.Length && i < read.Length; i++)
                {
                    if(write[i] != read[i])
                    {
                        Assert.Fail();
                    }
                }
            }
            catch
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Write_And_Read_Text_File()
        {
            string path = TestFile;
            if(!FileUtils.ExistsFile(path))
            {
                FileUtils.CreateFile(path);
            }
            string write = "IO Test String";
            try
            {
                FileUtils.WriteAllText(path, write);
                string read = FileUtils.ReadAllText(path);
                if(write != read)
                {
                    Assert.Fail();
                }
            }
            catch
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Existance_Mix()
        {
            // Verify that 'Exists' functions don't mix Files and Directories
            string path = TestGeneric;
            FileUtils.DeleteFile(path);
            FileUtils.DeleteDirectory(path);
            if(FileUtils.ExistsFile(path) || FileUtils.ExistsDirectory(path))
            {
                Assert.Fail();
            }

            FileUtils.CreateFile(path);
            if(!FileUtils.ExistsFile(path) || FileUtils.ExistsDirectory(path))
            {
                Assert.Fail();
            }
            FileUtils.DeleteFile(path);

            FileUtils.CreateDirectory(path);
            if(!FileUtils.ExistsDirectory(path) || FileUtils.ExistsFile(path))
            {
                Assert.Fail();
            }
            FileUtils.DeleteDirectory(path);
        }

        [Test]
        public void Existance_File_URL()
        {
            string path = TestURL;
            if(FileUtils.ExistsFile(path))
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Existance_Directory_URL()
        {
            string path = TestURL;
            if(FileUtils.ExistsDirectory(path))
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}