using System;
using System.Collections;
using System.Collections.Generic;

namespace Lockdown.Tests.Data
{
    public class SplitPostTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
            var nl = Environment.NewLine;

            yield return new object[] { $"--- {nl}ABC{nl}---{nl}XYZ", $"ABC{nl}", $"XYZ{nl}" };
            yield return new object[] { $"---{nl}ABC{nl}---{nl}XYZ", $"ABC{nl}", $"XYZ{nl}" };
            yield return new object[] { $"--- {nl}ABC{nl}---  {nl}XYZ", $"ABC{nl}", $"XYZ{nl}" };
        }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }