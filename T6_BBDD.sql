DROP DATABASE IF EXISTS T6_BBDD;
CREATE DATABASE T6_BBDD;

USE T6_BBDD;

CREATE TABLE Jugador (
	id INT PRIMARY KEY,
	nombre VARCHAR (30),
	contraseña VARCHAR (20),
	puntuacion INT
)ENGINE=InnoDB;

CREATE TABLE Partida(
	id INT PRIMARY KEY,
	nombreganador VARCHAR (30),
	nombreperdedor VARCHAR (30),
	golesganador INT,
	golesperdedor INT,
	duracion INT,
	fecha VARCHAR(30),
	hora VARCHAR(30)
)ENGINE=InnoDB;

CREATE TABLE RelacionJP(
	id_J INT,
	id_P INT,
	resultado VARCHAR(30),
	goles INT,
	FOREIGN KEY (id_J) REFERENCES Jugador(id),
	FOREIGN KEY (id_P) REFERENCES Partida(id)	
)ENGINE=InnoDB;

INSERT INTO Jugador VALUES (0, 'Juan', 'HOLA', 30);
INSERT INTO Jugador VALUES (1, 'Maria', 'CASA', 35);
INSERT INTO Partida VALUES (0, 'Maria', 'Juan', 3, 2, 120, '4/10/2019', '12:30');
INSERT INTO Jugador VALUES (2, 'Pedro', 'ABC', 21);
INSERT INTO Partida VALUES (1, 'Pedro', 'Juan', 4, 1, 220, '15/2/2020', '07:12');
INSERT INTO Partida VALUES (2, 'Maria', 'Pedro', 5, 4, 340, '12/3/2020', '19:42');
INSERT INTO RelacionJP VALUES (0, 0, '3-1', 3);
INSERT INTO RelacionJP VALUES (1, 0, '3-1', 1);
INSERT INTO RelacionJP VALUES (0, 2, '2-1', 1);
INSERT INTO RelacionJP VALUES (2, 2, '2-1', 2);
