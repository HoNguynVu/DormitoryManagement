namespace API.Services.Helpers
{
    public static class IdGenerator
    {
        /*
         * Hàm tiện ích tạo Suffix duy nhất (Semantic ID)
         * Lấy 10 ký tự đầu của một GUID mới, không dấu gạch, viết hoa
         */
        public static string GenerateUniqueSuffix()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        }

    }
}
