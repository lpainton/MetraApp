using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetraApplication
{
    public enum ErrorCondition
    {
        FirmwareFolderNotValidated,
    };

    public class MetraApplicationException : Exception 
    {
        public MetraApplicationException(string msg) : base(msg) { }
    }

    public delegate void ErrorHandler(MetraApplicationException mex, Form f);

    public class ErrorManager
    {
        FormMain MForm { get; set; }
        Dictionary<Type, ErrorHandler> ErrHandlers { get; set; }

        public ErrorManager(FormMain mForm)
        {
            MForm = mForm;
            ErrHandlers = new Dictionary<Type, ErrorHandler>();
        }

        public void HandleError(MetraApplicationException mex, Form f)
        {
            ErrorHandler myHandler;
            ErrHandlers.TryGetValue(mex.GetType(), out myHandler);
            if (myHandler != null)
            {
                myHandler(mex, f);
            }
            else
            {
                DefaultHandler(mex);
            }
        }

        private void DefaultHandler(MetraApplicationException mex)
        {
            throw mex;
        }

        public void RegisterHandler(Type t, ErrorHandler handler)
        {
            ErrHandlers.Add(t, handler);
        }
    }
}
