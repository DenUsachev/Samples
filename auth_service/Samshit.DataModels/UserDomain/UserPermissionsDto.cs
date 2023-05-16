namespace Samshit.DataModels.UserDomain
{
    public class UserPermissionsDto
    {
        private readonly int _rawValue;
        public bool IsActive { get; set; }

        public UserPermissionsDto(int rawValue)
        {
            _rawValue = rawValue;
            IsActive = _rawValue == 1;
        }

        public int ToRawValue() => IsActive ? 1 : 0;
    }
}
