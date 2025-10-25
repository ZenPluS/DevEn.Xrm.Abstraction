using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace DevEn.Xrm.Abstraction.Plugins.UnitTests.Stubs
{
    public class FakeTracingService
        : ITracingService
    {
        private readonly List<string> _messages = new List<string>();
        public IReadOnlyList<string> Messages => _messages;

        public void Trace(string format, params object[] args)
        {
            _messages.Add(args?.Length > 0 ? string.Format(format, args) : format);
        }
    }
}