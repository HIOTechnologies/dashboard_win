using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Security.Credentials;

namespace HIO.Backend.Edge
{

    class Edge 
    {
      

        public List<LoginFieldS> ReadPasswords()
        {
            var result = new List<LoginFieldS>();
            var vault = new PasswordVault();
            var credentials = vault.RetrieveAll();
            for (var i = 0; i < credentials.Count; i++)
            {
                PasswordCredential cred = credentials.ElementAt(i);
                cred.RetrievePassword();

                result.Add(new LoginFieldS
                {
                    url = HIOStaticValues.getTitleNameURI(cred.Resource).GetUTF8String(256),
                    userName = cred.UserName.GetUTF8String(64),
                    password = cred.Password.GetUTF8String(64),
                    title = HIOStaticValues.getTitleNameURI(cred.Resource).GetUTF8String(64) 
                });
            }
            return result;
        }
    }
}
