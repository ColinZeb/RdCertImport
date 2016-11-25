using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RdCertImport
{
    public class CertManager:IDisposable
    {
        public CertManager()
        {
            Store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            Store.Open(OpenFlags.ReadWrite);
        }
        X509Store Store { get; set; }

        public List<CertInfo> GetMyCert()
        {
           
            var list = Store.Certificates.Cast<X509Certificate2>()
                .Where(x => x.HasPrivateKey)
                .Select(x => new CertInfo
                {
                    FriendlyName =
                    x.FriendlyName,
                    SubjectName = x.SubjectName.Name,
                    Subject = x.Subject,
                    IssuerName = x.IssuerName.Name,
                    Useable = x.Verify(),
                    Hash = x.GetCertHashString()
                });
            Store.Close();
            return list.ToList();
        }
        public void ImportWithKey  (X509Certificate2 cert)
        {
            Store.Add(cert);
        }

        public void Remove(X509Certificate2 cert)
        {
            Store.Certificates.Remove(cert);
        }

        public bool Exist(X509Certificate2 cert)
        {
            return Store.Certificates.Contains(cert);
        }

        /// <summary>
        /// 打开没有密码的证书
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public X509Certificate2 OpenCert(string file)
        {
            return OpenCert(file, string.Empty);
        }

        public X509Certificate2 OpenCert(string file,string pass)
        {
            X509Certificate2 cert = new X509Certificate2(file, pass, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            return cert;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                Store.Close();
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~CertManager() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
