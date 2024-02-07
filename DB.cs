using Npgsql;
using System.Data;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spaceship;

public class DB
{

    public async void Connection()
    {

        string dbUri = "Host=localhost;Port=5455;Username=postgres;Password=postgres;Database=spaceship";

        await using var db = NpgsqlDataSource.Create(dbUri);

    }
    


}
