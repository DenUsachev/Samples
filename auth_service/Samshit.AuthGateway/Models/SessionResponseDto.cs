namespace Samshit.AuthGateway.Models
{
    public struct SessionResponseDto
    {
        public bool IsSuccess { get; set; }
        public string jwtToken { get; set; }
        public string Id { get; set; }
    }
}