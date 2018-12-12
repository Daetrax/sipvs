using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Windows;

namespace spracovanieInfo.Model
{
    class Validator
    {
        private XmlDocument doc;

        // namespaces
        private string dsNs = "http://www.w3.org/2000/09/xmldsig#";
        private string xzepNs = "http://www.ditec.sk/ep/signature_formats/xades_zep/v1.0";
        private string xadesNs = "http://uri.etsi.org/01903/v1.3.2#";

        private string canonicalization = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
        private string base64sig = "http://www.w3.org/2000/09/xmldsig#base64";

        private string digMethod;

        private string[] validSchemes = {
            "http://www.w3.org/2000/09/xmldsig#dsa-sha1",
            "http://www.w3.org/2000/09/xmldsig#rsa-sha1",
            "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
            "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384",
            "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512"
        };

        private Dictionary<string, string> validAlgorithms = new Dictionary<string, string>
            {
            {"http://www.w3.org/2000/09/xmldsig#sha1", "SHA-1"},
            {"http://www.w3.org/2001/04/xmldsig-more#sha224", "SHA-224"},
            {"http://www.w3.org/2001/04/xmlenc#sha256", "SHA-256"},
            {"http://www.w3.org/2001/04/xmldsig-more#sha384", "SHA-384"},
            {"http://www.w3.org/2001/04/xmlenc#sha512", "SHA-512"}
            };

        private Dictionary<string, string> validReferenceTypes = new Dictionary<string, string>
        {
            {"MANIFEST", "http://www.w3.org/2000/09/xmldsig#Manifest"},
            {"OBJECT", "http://www.w3.org/2000/09/xmldsig#Object"},
            {"SIGNEDPROPERTIES",  "http://uri.etsi.org/01903#SignedProperties"},
            {"SIGNATUREPROPERTIES", "http://www.w3.org/2000/09/xmldsig#SignatureProperties"}
        };

        private Dictionary<string, string> referenceElements = new Dictionary<string, string>
        {
            {"MANIFEST", "http://www.w3.org/2000/09/xmldsig#Manifest"},
            {"OBJECT", "http://www.w3.org/2000/09/xmldsig#Object"},
            {"SIGNEDPROPERTIES", "http://uri.etsi.org/01903#SignedProperties"},
            {"SIGNATUREPROPERTIES", "http://www.w3.org/2000/09/xmldsig#SignatureProperties"}
        };


        public Validator(XmlDocument doc)
        {
            this.doc = doc;
        }

        public bool validate()
        {
            this.validateDataEnvelope();
            this.validateXmlSignature();

            return true;
        }


        private Stream GenerateStreamFromString(string s)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(s);
            MemoryStream stream = new MemoryStream(byteArray);
            return stream;
        }

        private void errorNotify(string message)
        {
            MessageBoxResult result = MessageBox.Show($"Validation error: {message}",
                                                     "",
                                                     MessageBoxButton.OK,
                                                     MessageBoxImage.Warning);
        }

        private bool validateDataEnvelope()
        {
            var root = doc.GetElementsByTagName("xzep:DataEnvelope")[0];

            if (root == null)
            {
                errorNotify("xzep:DataEnvelope is missing");
                return false;
            }

            var xzepAttribute = root.Attributes.GetNamedItem("xmlns:xzep");
            if (xzepAttribute == null)
            {
                errorNotify("root element is missing the xmlns:xzep attribute");
                return false;
            }

            if (!xzepAttribute.InnerText.Equals(this.xzepNs))
            {
                errorNotify("xmlns:xzep attribute on the root element is invalid");
                return false;
            }

            var dsAttribute = root.Attributes.GetNamedItem("xmlns:ds");
            if (dsAttribute == null)
            {
                errorNotify("missing the xmlns:ds attribute on root");
                return false;
            }

            if (!dsAttribute.InnerText.Equals(dsNs))
            {
                errorNotify("xmlns:ds on the root is invalid");
                return false;
            }

            return true;
        }

        private bool validateXmlSignature()
        {
            var signatureMethod = this.doc.GetElementsByTagName("ds:SignatureMethod")[0];

            if (signatureMethod == null)
            {
                errorNotify("Missing ds:SignatureMethod in ds:Signature");
                return false;
            }

            var algorithmAttribute = signatureMethod.Attributes.GetNamedItem("Algorithm");
            if (algorithmAttribute == null)
            {
                errorNotify("ds:SignatureMethod does not contain Algorithm");
                return false;
            }

            string signatureScheme = algorithmAttribute.InnerText;
            if (!validSchemes.Contains(signatureScheme))
            {
                errorNotify("ds:SignatureMethod Algorithm is not supported");
                return false;
            }

            var canonicalizationMethod = this.doc.GetElementsByTagName("ds:CanonicalizationMethod")[0];

            if (canonicalizationMethod == null)
            {
                errorNotify("ds:Signature element is missing the ds:CanonicalizationMethod");
                return false;
            }

            algorithmAttribute = canonicalizationMethod.Attributes.GetNamedItem("Algorithm");
            if (algorithmAttribute == null)
            {
                errorNotify("CanonicalizationMethod does not have algorithm attribute");
                return false;
            }

            if (!algorithmAttribute.InnerText.Equals("http://www.w3.org/TR/2001/REC-xml-c14n-20010315"))
            {
                errorNotify("Canonicalization algorithm is invalid");
                return false;
            }

            //------------------ 3 TRANSFORM AND DIGEST METHOD ------------------
            #region transform and digest method
            var signedInfo = this.doc.GetElementsByTagName("ds:SignedInfo")[0];
            if (signedInfo == null)
            {
                errorNotify("ds:SignedInfo not found");
                return false;
            }

            var children = signedInfo.ChildNodes;
            for (int i = 0; i < children.Count; i++)
            {
                var n = children[i];

                if (n.GetType() == typeof(XmlElement) && n.Name.Equals("ds:Reference"))
                {
                    XmlElement e = (XmlElement)n;
                    var digest = e.GetElementsByTagName("ds:DigestMethod")[0];
                    String digestAlgorighm = digest.Attributes.GetNamedItem("Algorithm").InnerText;
                    if (!this.validAlgorithms.ContainsKey(digestAlgorighm))
                    {
                        errorNotify("ds:DigestMethod algorithm in SignedInfo is not supported");
                        return false;
                    }

                    var transforms = e.GetElementsByTagName("ds:Transforms")[0].ChildNodes;
                    for (int j = 0; j < transforms.Count; j++)
                    {
                        var transform = transforms[j];
                        if (!transform.Name.Equals("ds:Transform"))
                        {
                            continue;
                        }

                        String transformAlgorithm = transform.Attributes.GetNamedItem("Algorithm").InnerText;
                        if (!transformAlgorithm.Equals(canonicalization))
                        {
                            errorNotify("One or more transforms are invalid");
                            return false;
                        }
                    }

                }
            }
            #endregion

            #region Core validation
            var signatureValue = this.doc.GetElementsByTagName("ds:SignatureValue")[0];
            var keyInfo = this.doc.GetElementsByTagName("ds:KeyInfo")[0];

            if (signedInfo == null || signatureValue == null)
            {
                errorNotify("ds:SignedInfo or ds:SignatureValue are missing");
                return false;
            }

            if (signedInfo.GetType() == typeof(XmlElement))
            {
                var signedInfoElement = (XmlElement)signedInfo;
                var references = signedInfoElement.GetElementsByTagName("ds:Reference");
                for (int i = 0; i < references.Count; i++)
                {
                    var n = references[i];
                    if (n.GetType() == typeof(XmlElement))
                    {
                        var reference = (XmlElement)n;
                        var refType = reference.GetAttribute("Type");

                        var digestMethod = ((XmlElement)reference.GetElementsByTagName("ds:DigestMethod")[0]).GetAttribute("Algorithm");
                        this.digMethod = digestMethod;

                        if (refType.Equals(this.validReferenceTypes["MANIFEST"]))
                        {
                            var something = reference.Attributes.GetNamedItem("URI");
                            var manifestId = reference.Attributes.GetNamedItem("URI").InnerText.Substring(1);
                            XmlNamespaceManager mn = new XmlNamespaceManager(doc.NameTable);
                            mn.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");


                            var manifests = this.doc.SelectNodes($"//*[@Id='{manifestId}']");

                            var manifest = manifests[0];
                            

                            if (manifest == null)
                            {
                                errorNotify($"Element {manifestId} not found");
                                return false;
                            }

                            var manifestContent = manifest.ToString();

                            try
                            {
                                XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
                                transform.Algorithm = SignedXml.XmlDsigC14NTransformUrl;
                                var a = transform.InputTypes;

                                var streamManifest = GenerateStreamFromString(manifest.OuterXml);
                                transform.LoadInput(streamManifest);
                                var ms = (MemoryStream)transform.GetOutput(typeof(Stream));

                                var digestIdentifier = validAlgorithms[digestMethod];

                                if (digestIdentifier == null)
                                {
                                    errorNotify("Digest method not supported");
                                    return false;
                                }

                                var digested = SHA256.Create().ComputeHash(ms);
                                
                                var ourDigest = Convert.ToBase64String(digested);
                                var theirDigest = reference.GetElementsByTagName("ds:DigestValue")[0].InnerText;

                                if (!ourDigest.Equals(theirDigest))
                                {
                                    errorNotify("Digest value mismatch for references");
                                    return false;
                                }

                            }
                            catch (Exception e)
                            {
                                errorNotify("Unable to validate signature.");
                            }
                        }
                    }
                }

                // signature validation
                var _cert = ((XmlElement)keyInfo).GetElementsByTagName("ds:X509Certificate")[0].InnerText;
                try
                {

                    XmlDsigC14NTransform transform = new XmlDsigC14NTransform();

                    transform.LoadInput(GenerateStreamFromString(signedInfo.OuterXml));
                    var cannonSignedInfo = (MemoryStream)transform.GetOutput(typeof(Stream));

                    var certBytes = Convert.FromBase64String(_cert);
                    SubjectPublicKeyInfo ski = X509CertificateStructure.GetInstance(Asn1Object.FromByteArray(certBytes)).SubjectPublicKeyInfo;
                    AsymmetricKeyParameter pk = PublicKeyFactory.CreateKey(ski);

                    var algString = this.validAlgorithms[this.digMethod];

                    var signatureBytes = Convert.FromBase64String(signatureValue.InnerText);

                    switch (ski.AlgorithmID.ObjectID.Id)
                    {
                        case "1.2.840.10040.4.1":
                            algString += "withdsa";
                            break;
                        case "1.2.840.113549.1.1.1":
                            algString += "withrsa";
                            break;

                        default:
                            Console.WriteLine("Err.");
                            break;

                    }

                    ISigner verif = SignerUtilities.GetSigner(algString);
                    verif.Init(false, pk);
                    verif.BlockUpdate(cannonSignedInfo.ToArray(), 0, cannonSignedInfo.ToArray().Length);
                    var signatureVerificationResult = verif.VerifySignature(signatureBytes);

                    if (!signatureVerificationResult)
                    {
                        errorNotify("Signature verification failed.");
                    }

                }
                catch (Exception e)
                {
                    errorNotify("Unable to validate signature.");
                }
            }


            #endregion


            #region signature element validation
            var _sig = this.doc.SelectSingleNode($"//ds:Signature");

            if (_sig == null)
            {
                errorNotify("ds:Signature element is missing");
                return false;
            }
            var signature = (XmlElement) _sig;

            String id = signature.GetAttribute("Id");
            if (id.Equals(""))
            {
                errorNotify("ds:Signature element does not have Id");
                return false;
            }

            String ds_ns = signature.GetAttribute("xmlns:ds");
            if (ds_ns.Equals(""))
            {
                errorNotify("No xmlns:ds attribute in ds:Signature");
                return false;
            }

            if (!ds_ns.Equals(dsNs))
            {
                errorNotify("ds:Signature element xmlns:ds attribute value is invalid");
                return false;
            }

            #endregion

            #region signatureValue valid
            var _sigVal = this.doc.SelectSingleNode($"//ds:SignatureValue");

            if (_sigVal == null)
            {
                errorNotify("ds:SignatureValue element is missing");
                return false;
            }
            var signatureValueElement = (XmlElement)_sigVal;

            var signatureValueId = signatureValueElement.GetAttribute("Id");
            if (id.Equals(""))
            {
                errorNotify("ds:SignatureValue element is does not have Id");
                return false;
            }
            #endregion

            #region signedInfo reference
            var SignedInfoReferences = this.doc.SelectNodes($"//ds:SignedInfo/ds:Reference");

            if (SignedInfoReferences.Count == 0)
            {
                errorNotify("No references in the ds:SignedInfo ");
                return false;
            }

            for (int i = 0; i < SignedInfoReferences.Count; i++)
            {
                var reference = (XmlElement)SignedInfoReferences[i];

                String Id = reference.GetAttribute("Id");
                if (Id.Equals(""))
                {
                    errorNotify("ds:SignedInfo reference is missing the Id");
                    return false;
                }

                String type = reference.GetAttribute("Type");
                if (type.Equals(""))
                {
                    errorNotify("Reference is missing the URI attribute {Id}");
                    return false;
                }

                String URI = reference.GetAttribute("URI");
                if (URI.Equals(""))
                {
                    errorNotify($"Reference is missing the URI attribute {Id}");
                    return false;
                }
                URI = URI.Substring(1);
                var referencedElement = this.doc.SelectSingleNode($"//*[@Id='{URI}']");

                if (referencedElement == null)
                {
                    errorNotify($"Reference {URI} in ds:SignedInfo not found");
                    return false;
                }
                var name = referencedElement.Name;
                String requiredType = null;
                switch (name)
                {
                    case ("xades:SignedProperties"):
                        requiredType = referenceElements["SIGNEDPROPERTIES"];
                        break;
                    case ("ds:SignatureProperties"):
                        requiredType = referenceElements["SIGNATUREPROPERTIES"];
                        break;
                    case ("ds:KeyInfo"):
                        requiredType = referenceElements["OBJECT"];
                        break;
                    default:
                        requiredType = referenceElements["MANIFEST"];
                        break;
                }

                if (!type.Equals(requiredType))
                {
                    errorNotify("ds:SignedInfo reference type does not match");
                    return false;
                }
            }
            #endregion


            return true;
        }

        private bool validateTimestamp()
        {
            return false;
        }

        private bool validateCertificate()
        {
            return false;
        }
   

    }
}
