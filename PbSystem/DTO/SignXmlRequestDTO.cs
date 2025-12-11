namespace PbSystem.DTO
{
    public class SignXmlRequestDto
    {
        public string UnsignedXml { get; set; } = string.Empty;
        public string GivenName { get; set; } = "Demo";
        public string Surname { get; set; } = "User";
        public string SerialNumber { get; set; } = "DEMO-0001";
        public string CommonName { get; set; } = "Demo Certificate";

        // If true, remove any existing <Signature> elements before signing.
        public bool RemoveExistingSignature { get; set; } = false;
    }

}
