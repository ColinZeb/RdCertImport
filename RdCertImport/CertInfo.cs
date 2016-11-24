using System.Security.Cryptography.X509Certificates;

namespace RdCertImport
{
    public class CertInfo
    {
        public string FriendlyName { get; set; }
        public string Hash { get; internal set; }
        public string IssuerName { get; set; }
        public string Subject { get; set; }
        public string SubjectName { get; set; }
        public bool Useable { get; set; }
    }
}