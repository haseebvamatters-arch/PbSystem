using Microsoft.AspNetCore.Mvc;
using PbSystem.DTO;
using PbSystem.Interfaces;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace PbSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SigningController : ControllerBase
    {
        private readonly ISignatureService _signatureService;

        public SigningController(ISignatureService signatureService)
        {
            _signatureService = signatureService;
        }

        [HttpPost("sign-xml-demo")]
        public IActionResult SignXmlDemo([FromBody] SignXmlRequestDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Request body is required." });

            if (string.IsNullOrWhiteSpace(dto.UnsignedXml))
                return BadRequest(new { message = "unsignedXml is required." });

            // Trim and remove BOM
            string raw = dto.UnsignedXml.Trim();
            if (raw.Length > 0 && raw[0] == '\uFEFF') raw = raw.Substring(1);

            // Basic check
            if (raw.Length == 0 || raw[0] != '<')
                return BadRequest(new { message = "UnsignedXml must contain XML starting with '<'." });

            // Load xml
            XmlDocument xmlDoc = new XmlDocument { PreserveWhitespace = true };
            try
            {
                xmlDoc.LoadXml(raw);
            }
            catch (XmlException xe)
            {
                return BadRequest(new { message = "Invalid XML: parsing failed.", detail = xe.Message });
            }

            // If caller asked to remove existing signatures, or you want to reject them:
            var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

            var signatureNodes = xmlDoc.SelectNodes("//ds:Signature", nsmgr);
            if (signatureNodes != null && signatureNodes.Count > 0)
            {
                if (!dto.RemoveExistingSignature)
                {
                    // Option A: reject input that is already signed
                    return BadRequest(new { message = "Input XML already contains a Signature element. Set RemoveExistingSignature=true to strip it before signing." });
                }

                // Option B: remove all found Signature nodes
                foreach (XmlNode node in signatureNodes)
                {
                    node.ParentNode?.RemoveChild(node);
                }
            }

            // After removal (if any), get cleaned xml string
            string cleanedXml = xmlDoc.OuterXml;

            // Create demo cert and sign
            using var cert = SelfSignedCertificateForSignatureBuilder
                                .Create()
                                .WithGivenName(dto.GivenName)
                                .WithSurname(dto.Surname)
                                .WithSerialNumber(dto.SerialNumber)
                                .WithCommonName(dto.CommonName)
                                .Build();

            string signedXml;
            try
            {
                signedXml = _signatureService.Sign(cleanedXml, cert);
            }
            catch (Exception ex)
            {
                // log the exception in real app
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Signing failed." });
            }

            // Return raw signed XML (Make likely expects this)
            return Content(signedXml, "application/xml");
        }


    }
}
