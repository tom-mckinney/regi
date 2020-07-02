using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Test.Extensions
{
    public static class MockExtensions
    {
        public static void VerifyAll(this IEnumerable<Mock> mocks, MockBehavior behavior = MockBehavior.Strict)
        {
            if (mocks == null)
            {
                throw new ArgumentNullException(nameof(mocks));
            }

            foreach (var mock in mocks)
            {
                switch (behavior)
                {
                    case MockBehavior.Strict:
                        mock.VerifyAll();
                        break;
                    default:
                        mock.Verify();
                        break;
                }
            }
        }
    }
}
