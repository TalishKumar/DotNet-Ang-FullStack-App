using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;

// Contract between itself and any class that implements it
// Any class that implements this interface will implement the interfaces properties, methods, and events

// Does not contain any implementation logic
// Only contains the signatures of the functionality that the interface provides

namespace API.Interfaces
{
    public interface ITokenService {
        string CreateToken(AppUser user);
    }
}