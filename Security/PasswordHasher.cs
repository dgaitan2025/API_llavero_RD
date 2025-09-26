using Isopoh.Cryptography.Argon2; // Si NO usarás Argon2, puedes quitar esta using y el bloque correspondiente

namespace ProyDesaWeb2025.Security;

public static class PasswordHasher
{
    public static bool Verify(string plain, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash) || string.IsNullOrEmpty(plain)) return false;

        // BCrypt: $2a$ / $2b$
        if (hash.StartsWith("$2a$") || hash.StartsWith("$2b$"))
            return BCrypt.Net.BCrypt.Verify(plain, hash);

        // Argon2id (opcional): $argon2id$
        if (hash.StartsWith("$argon2id$"))
        {
            try { return Argon2.Verify(hash, plain); }
            catch { return false; }
        }

        return false;
    }
}