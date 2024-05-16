using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi
{
    public class JsonFavoriteAddress
    {
        public JsonFavoriteAddress(string id_favoriteAddresse, string user_favoriteAddresse, string coorders_favoriteAddresse, string name_favoriteAddresse)
        {
            Id = Convert.ToInt32(id_favoriteAddresse);
            User = Convert.ToInt32(user_favoriteAddresse);
            Coorders = coorders_favoriteAddresse;
            Name = name_favoriteAddresse;
        }

        public int Id { get; private set; }
        public int User { get; private set; }
        public string Coorders { get; private set; }
        public string Name { get; private set; }
    }

    public class JsonFavoriteAddresses
    {
        public JsonFavoriteAddresses(List<JsonFavoriteAddress> addresses) 
        { 
            Addresses = addresses;
        }

        public List<JsonFavoriteAddress> Addresses { get; private set; }
    }
}
