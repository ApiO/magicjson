using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MagicJson.Tools;

namespace MagicJson.Data
{
    public class ValidationItem
    {

        public string File { get; private set; }
        public Utils.ValidationState Validation { get; set; }

        private Result _result;
        private bool _hasResult;
        private string _formatedResult;

        public ValidationItem(string file, Utils.ValidationState validation)
        {
            File = file;
            Validation = validation;
        }

        public void SetResult(Result result)
        {
            _hasResult = true;
            _result = result;

            Validation = result.Valid
                    ? Utils.ValidationState.Success
                    : Utils.ValidationState.Error;

            var source = System.IO.File.ReadAllBytes(File);
            _formatedResult = string.Format("File: {0}\r\n\r\n{1}", File, Utils.FormatResult(_result, source));
        }

        public bool HasResult()
        {
            return _hasResult;
        }

        public override string ToString()
        {
            return _formatedResult;
        }
    }
}