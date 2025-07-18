using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace ID.AcadNet.Infrastructure.Core
{
    class Cert
    {
        private X509Certificate GetCertificate()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySerialNumber, "123456", true);
            store.Close();
            return certs[0];
        }

        public static bool addCertToStore(X509Certificate2 cert, StoreName st, StoreLocation sl)
        {
            bool bRet = false;

            try
            {
                X509Store store = new X509Store(st, sl);
                store.Open(OpenFlags.ReadWrite);

                if (cert != null)
                {
                    byte[] pfx = cert.Export(X509ContentType.Pfx);
                    cert = new X509Certificate2(pfx, (string)null, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);

                    //if (!certExists(store, cert.SubjectName.Name))
                    //{
                    //    store.Add(cert);
                    //    bRet = true;
                    //}
                }
                store.Close();
            }
            catch
            {

            }

            return bRet;
        }

        public void getCert() {
            //X509Store userCaStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            X509Store userCaStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);

            try
            {
                userCaStore.Open(OpenFlags.ReadOnly);

                //X509Certificate2Collection findResult = certificatesInStore.Find(X509FindType.FindByIssuerName, "RootCertificate", false);
                //X509Certificate2Collection findResult = certificatesInStore.Find(X509FindType.FindBySubjectName, "localtestclientcert", true);
                X509Certificate2Collection certificatesInStore = userCaStore.Certificates;

                foreach (X509Certificate2 cert in certificatesInStore)
                {
                    Debug.WriteLine(cert.GetExpirationDateString());
                    Debug.WriteLine(cert.Issuer);
                    Debug.WriteLine(cert.GetEffectiveDateString());
                    Debug.WriteLine(cert.GetNameInfo(X509NameType.SimpleName, true));
                    Debug.WriteLine(cert.HasPrivateKey);
                    Debug.WriteLine(cert.SubjectName.Name);
                    Debug.WriteLine("-----------------------------------");
                }
            }
            finally
            {
                userCaStore.Close();
            }
        }

        private static void RunClientCertValidation()
        {
            X509Store userCaStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            try
            {
                userCaStore.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certificatesInStore = userCaStore.Certificates;
                X509Certificate2Collection findResult = certificatesInStore.Find(X509FindType.FindBySubjectName, "localtestclientcert", true);
                foreach (X509Certificate2 cert in findResult)
                {
                    X509Chain chain = new X509Chain();
                    X509ChainPolicy chainPolicy = new X509ChainPolicy()
                    {
                        RevocationMode = X509RevocationMode.Online,
                        RevocationFlag = X509RevocationFlag.EntireChain
                    };
                    chain.ChainPolicy = chainPolicy;
                    if (!chain.Build(cert))
                    {
                        foreach (X509ChainElement chainElement in chain.ChainElements)
                        {
                            foreach (X509ChainStatus chainStatus in chainElement.ChainElementStatus)
                            {
                                Debug.WriteLine(chainStatus.StatusInformation);
                            }
                        }
                    }
                }
            }
            finally
            {
                userCaStore.Close();
            }
        }
    }
}
