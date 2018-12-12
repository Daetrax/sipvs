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

        //private X509Certificate loadCertificate(String asn1)
        //{
        //    byte[] data = Convert.FromBase64String(asn1);
        //    ByteArrayInputStream inStream = new ByteArrayInputStream(data);
        //    Asn1InputStream derin = new ASN1InputStream(inStream);
        //    ASN1Primitive certInfo = derin.readObject();
        //    ASN1Sequence seq = ASN1Sequence.getInstance(certInfo);
        //    return new X509CertificateObject(Certificate.getInstance(seq));
        //}

        //private XmlNodeList findByAttributeValue()
        //{
        //    IEnumerable<XElement> list1 =
        //    from el in this.doc.Elements()
        //    where el.Attribute("Select") != null
        //    select el;


        //}

        private bool validateDataEnvelope()
        {
            var root = doc.GetElementsByTagName("xzep:DataEnvelope")[0];

            if (root == null)
            {
                //this.ERROR_SPECIFIC = "The root element xzep:DataEnvelope is missing.";
                return false;
            }

            var xzepAttribute = root.Attributes.GetNamedItem("xmlns:xzep");
            if (xzepAttribute == null)
            {
                //this.ERROR_SPECIFIC = "The root element is missing the xmlns:xzep attribute.";
                return false;
            }

            if (!xzepAttribute.InnerText.Equals(this.xzepNs))
            {
                //this.ERROR_SPECIFIC = "The value of the xmlns:xzep attribute on the root element is invalid. Found: " + xzepAttribute.getTextContent();
                return false;
            }

            var dsAttribute = root.Attributes.GetNamedItem("xmlns:ds");
            if (dsAttribute == null)
            {
                //this.ERROR_SPECIFIC = "The root element is missing the xmlns:ds attribute.";
                return false;
            }

            if (!dsAttribute.InnerText.Equals(dsNs))
            {
                //this.ERROR_SPECIFIC = "The value of the xmlns:ds attribute on the root element is invalid. Found: " + dsAttribute.getTextContent();
                return false;
            }

            return true;
        }

        private bool validateXmlSignature()
        {
            var signatureMethod = this.doc.GetElementsByTagName("ds:SignatureMethod")[0];

            if (signatureMethod == null)
            {
                //this.ERROR_SPECIFIC = "The ds:Signature element is missing the ds:SignatureMethod element.";
                return false;
            }

            var algorithmAttribute = signatureMethod.Attributes.GetNamedItem("Algorithm");
            if (algorithmAttribute == null)
            {
                //this.ERROR_SPECIFIC = "The ds:SignatureMethod element is missing the Algorithm attribute.";
                return false;
            }

            string signatureScheme = algorithmAttribute.InnerText;
            if (!validSchemes.Contains(signatureScheme))
            {
                //this.ERROR_SPECIFIC = "The ds:SignatureMethod Algorithm is not supported. Found: " + signatureScheme;
                return false;
            }

            var canonicalizationMethod = this.doc.GetElementsByTagName("ds:CanonicalizationMethod")[0];

            if (canonicalizationMethod == null)
            {
                //this.ERROR_SPECIFIC = "The ds:Signature element is missing the ds:CanonicalizationMethod element.";
                return false;
            }

            algorithmAttribute = canonicalizationMethod.Attributes.GetNamedItem("Algorithm");
            if (algorithmAttribute == null)
            {
                //this.ERROR_SPECIFIC = "The ds:CanonicalizationMethod element is missing the Algorithm attribute.";
                return false;
            }

            if (!algorithmAttribute.InnerText.Equals("http://www.w3.org/TR/2001/REC-xml-c14n-20010315"))
            {
                //this.ERROR_SPECIFIC = "The ds:CanonicalizationMethod Algorithm is not supported. Found: " + algorithmAttribute.getTextContent();
                return false;
            }

            //------------------ 3 TRANSFORM AND DIGEST METHOD ------------------
            #region transform and digest method
            var signedInfo = this.doc.GetElementsByTagName("ds:SignedInfo")[0];
            if (signedInfo == null)
            {
                //this.ERROR_SPECIFIC = "The ds:SignedInfo element is missing.";
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
                        //this.ERROR_SPECIFIC = "A ds:DigestMethod Algorithm in a ds:SignedInfo reference is not supported. Found: " + digestAlgorighm;
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
                            //this.ERROR_SPECIFIC = "One of the transforms specified in a ds:SignedInfo reference is invalid. Found: " + transformAlgorithm;
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
                //this.ERROR_SPECIFIC = "The ds:SignedInfo or ds:SignatureValue element are missing.";
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

                            var manifestId = reference.Attributes.GetNamedItem("URI").InnerText.Substring(1);
                            //var manifests = this._findNodesByAttributeValue("Id", manifestId);
                            var something = $"/nodes/node/attribute[@Id=\"{manifestId}\"";
                            XmlNamespaceManager mn = new XmlNamespaceManager(doc.NameTable);
                            mn.AddNamespace("xmlns:ds", "http://www.w3.org/2000/09/xmldsig#");
                            var manifests = this.doc.SelectSingleNode($"//Manifest[@Id='{manifestId}']");
                            var manifest = (XmlElement)manifests;

                            if (manifest == null)
                            {
                                //this.ERROR_SPECIFIC = "Element with Id " + manifestId + " not found.";
                                return false;
                            }

                            //var manifestContent = nodeToString(manifest); 
                            var manifestContent = manifest.ToString();

                            try
                            {

                                XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
                                transform.LoadInput(manifest);
                                var ms = (MemoryStream)transform.GetOutput(typeof(Stream));
                                var canonicalizedNode = new StreamReader(ms).ReadToEnd();

                                //byte[] canonXmlBytes = Transform.Canonicalize(manifestContent);
                                //String canonXmlString = new String(canonXmlBytes);
                                var digestIdentifier = validAlgorithms[digestMethod];

                                if (digestIdentifier == null)
                                {
                                    //this.ERROR_SPECIFIC = String.format("Digest method %s is not supported.", digestMethod);
                                    return false;
                                }
                                //var digest = MessageDigest.getInstance(digestIdentifier);
                                //byte[] digested = digest.digest(canonXmlBytes);

                                var digested = SHA1Managed.Create().ComputeHash(ms);

                                var ourDigest = Convert.ToBase64String(digested);
                                var theirDigest = reference.GetElementsByTagName("ds:DigestValue")[0].InnerText;

                                if (!ourDigest.Equals(theirDigest))
                                {
                                    //this.ERROR_SPECIFIC = "Digest value mismatch for reference in the ds:SignedInfo element. Ref. Id: " + ref.getAttribute("Id");
                                    return false;
                                }

                            }
                            catch (Exception e)
                            {
                                //e.printStackTrace();
                                //this.ERROR_SPECIFIC = "Something went wrong...";
                                return false;
                            }
                        }
                    }
                }

                var _cert = ((XmlElement)keyInfo).GetElementsByTagName("ds:X509Certificate")[0].InnerText;
                try
                {
                    //var certBytes = Encoding.Default.GetBytes(_cert);
                    //X509Certificate certificate = new X509Certificate(certBytes);
                    //loadCertificate(_cert);

                    //PublicKey key = certificate.getPublicKey();

                    //String signedInfoContent = nodeToString(signedInfo);
                    //String signedInfoContent = signedInfo.ToString();

                    XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
                    transform.LoadInput(signedInfo);
                    var cannonSignedInfo = (MemoryStream)transform.GetOutput(typeof(Stream));

                    var certBytes = Convert.FromBase64String(_cert);
                    SubjectPublicKeyInfo ski = X509CertificateStructure.GetInstance(Asn1Object.FromByteArray(certBytes)).SubjectPublicKeyInfo;
                    AsymmetricKeyParameter pk = PublicKeyFactory.CreateKey(ski);

                    var algString = this.validAlgorithms[this.digMethod];

                    var signature = Convert.FromBase64String(signatureValue.InnerText);

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
                    var signatureVerificationResult = verif.VerifySignature(signature);

                    if (!signatureVerificationResult)
                    {
                        Console.WriteLine("Signature verification failed.");
                    }

                    //var canonicalizedNode = new StreamReader(ms).ReadToEnd();

                    //byte[] cannonSignedInfo = canonicalize(signedInfoContent);

                    //Signature signer = Signature.getInstance(certificate.getSigAlgName(), "BC");

                    //signer.initVerify(certificate.getPublicKey());
                    //signer.update(cannonSignedInfo);

                    //if (!signer.verify(Base64.getDecoder().decode(signatureValue.getTextContent().getBytes())))
                    //{
                    //    this.ERROR_SPECIFIC = String.format("Signature verification failed. (Element: ds:SignatureValue, Certificate type: %s, Algorithm: %s).", certificate.getType(), certificate.getSigAlgName());
                    //    return false;
                    //}
                }
                catch (Exception e)
                {
                    //e.printStackTrace();
                    //this.ERROR_SPECIFIC = "Something went wrong...";
                    //return false;
                }
            }

            return true;

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
