import Janrain.OpenId.Server
import NUnit.Framework
import System
import System.IO
import System.Text.RegularExpressions

abstract class TrustRootTest:
    virtual def Run():
        pass


class ParseTest(TrustRootTest):
    case as string
    sanity as string
    
    def constructor(sanity, case):
        self.case = case
        self.sanity = sanity

    override def Run():
        tr as TrustRoot = null
        try:
            tr = TrustRoot(self.case)
        except e as ArgumentException:
            pass

        msg = "${self.case} - ${self.sanity}"
        if self.sanity == 'sane':
            Assert.AreEqual(tr.IsSane, true, msg)
        elif self.sanity == 'insane':
            Assert.AreEqual(tr.IsSane, false, msg)
        else:
            Assert.AreEqual(tr, null)

class MatchTest(TrustRootTest):
    match as bool
    tr as string
    rt as string

    def constructor(match as bool, line as string):
        tr, rt = /\s+/.Split(line)
        self.match = match

    override def Run():
        tr as TrustRoot = null
        try:
            tr = TrustRoot(self.tr)
        except e as ArgumentException:
            Assert.Fail("TrustRoot couldn't parse ${self.tr}")
        
        msg = "${self.match} - ${self.tr} - ${self.rt}"
        uri as Uri = null
        try:
            uri = Uri(self.rt)
        except e as UriFormatException:
            return
            #Assert.Fail("${self.rt} doesn't parse as a Uri")

        x = tr.ValidateUrl(uri)
        if self.match:
            Assert.IsTrue(x, msg)
        else:
            Assert.IsFalse(x, msg)

[TestFixture]
class TrustRootTests:
    text as string
    tests as List

    def GetTests(t as System.Type, grps, head as string, dat as string):
        tests = []

        top = head.Trim()
        delim_re = Regex('-' * 40 + '\n')
        gdat = array(s.Trim() for s in delim_re.Split(dat))

        i = 1
        for x in grps:
            n, desc = /:\s/.Split(gdat[i])
            cases = /\n/.Split(gdat[i + 1])
            for case in cases:
                obj = t(x, case)
                tests.Add(obj)
            i += 2

        return tests

    
    [SetUp]
    def SetUp():
        fs = FileStream("../test/openid/trustroot.txt", FileMode.Open,
                        FileAccess.Read)
        sr = StreamReader(fs)
        text = sr.ReadToEnd()
        sr.Close()
        fs.Close()

        delim_re = Regex('=' * 40 + '\n')
        _, ph, pdat, mh, mdat = array(s.Trim() for s in delim_re.Split(text))
        
        tests = []
        tests += GetTests(ParseTest, ('bad', 'insane', 'sane'), ph, pdat)
        tests += GetTests(MatchTest, [true, false], mh, mdat)
    
    [Test]
    def GetX():
        for test as TrustRootTest in tests:
            test.Run()


# class ParseTest: #(unittest.TestCase):
#     def constructor(self, sanity, desc, case):
#         unittest.TestCase.__init__(self)
#         self.desc = desc + ': ' + repr(case)
#         self.case = case
#         self.sanity = sanity

#     def shortDescription(self):
#         return self.desc

#     def runTest(self):
#         tr = TrustRoot.parse(self.case)
#         if self.sanity == 'sane':
#             assert tr.isSane(), self.case
#         elif self.sanity == 'insane':
#             assert not tr.isSane(), self.case
#         else:
#             assert tr is None


# def parseTests(data):
#     parts = map(str.strip, data.split('=' * 40 + '\n'))
#     assert not parts[0]
#     _, ph, pdat, mh, mdat = parts

#     tests = []
#     tests.extend(getTests(_ParseTest, ['bad', 'insane', 'sane'], ph, pdat))
#     tests.extend(getTests(_MatchTest, [1, 0], mh, mdat))
#     return tests

# def pyUnitTests():
#     here = os.path.dirname(os.path.abspath(__file__))
#     test_data_file_name = os.path.join(here, 'trustroot.txt')
#     test_data_file = file(test_data_file_name)
#     test_data = test_data_file.read()
#     test_data_file.close()

#     tests = parseTests(test_data)
#     return unittest.TestSuite(tests)

# if __name__ == '__main__':
#     suite = pyUnitTests()
#     runner = unittest.TextTestRunner()
#     runner.run(suite)
