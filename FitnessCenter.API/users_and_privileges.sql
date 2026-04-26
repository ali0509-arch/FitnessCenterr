-- ============================================================
-- SQL script til oprettelse af dedikeret app bruger
-- og tildeling af rettigheder (principle of least privilege)
-- ============================================================

-- Slet bruger hvis den allerede eksisterer
DROP USER IF EXISTS 'fitness_user'@'%';

-- Opret dedikeret app bruger
CREATE USER 'fitness_user'@'%' IDENTIFIED BY 'StrongPass123!';

-- Giv kun nødvendige rettigheder (ikke DROP, ALTER, etc.)
GRANT SELECT, INSERT, UPDATE, DELETE ON kunforhustlers_dk_db.*
    TO 'fitness_user'@'%';

-- Anvend ændringerne
FLUSH PRIVILEGES;

-- Verificer rettigheder
SHOW GRANTS FOR 'fitness_user'@'%';