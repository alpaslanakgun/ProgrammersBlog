using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammersBlog.Services.Utilities
{
    public static class Messages
    {
        public static class CategoryMessage
        {

            public static string NotFound(bool isPlural)
            {
                if (isPlural)
                {
                    return "Hiç bir Kategori Bulunamadı";
                }
                else
                {
                    return "Böyle bir kategori bulunamadı";
                }
            }

            public static string Add(string categoryName)
            {

                return $"{categoryName} adlı kategori başarı ile güncellenmiştir.";

            }
            public static string Update(string categoryName)
            {
                return $"{categoryName} adlı kategori başarı ile güncellenmiştir.";
            }
            public static string Delete(string categoryName)
            {
                return $"{categoryName} adlı kategori başarı ile silinmiştir.";
            }
            public static string HardDelete(string categoryName)
            {
                return $"{categoryName} adlı kategori başarı ile silinmiştir.";
            }

        }

        public static class ArticleMessage
        {

            public static string NotFound(bool isPlural)
            {

                if (isPlural)
                {
                    return "Makaleler Bulunamadı";
                }
                else
                {
                    return "Makale Bulunamadı";
                }
            }

            public static string NotFoundById(int articleId)
            {
                return $"{articleId} makale koduna ait böyle bir makale bulunamadı.";
            }
            public static string Add(string articleTitle)
            {
                return $"{articleTitle} başlıklı makale başarıyla eklenmiştir.";
            }

            public static string Update(string articleTitle)
            {
                return $"{articleTitle} başlıklı makale başarıyla güncellenmiştir.";
            }
            public static string Delete(string articleTitle)
            {
                return $"{articleTitle} başlıklı makale başarıyla silinmiştir.";
            }
            public static string HardDelete(string articleTitle)
            {
                return $"{articleTitle} başlıklı makale başarıyla veritabanından silinmiştir.";
            }



        }
    }
}
