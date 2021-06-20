CREATE TABLE User (
    userId varchar(20) NOT NULL,
    lastName varchar(100) NOT NULL,
    firstName varchar(100) NOT NULL,
    birthday date NOT NULL,
    createdAt timestamp NOT NULL,
    updatedAt timestamp NOT NULL,
    PRIMARY KEY(userId, updatedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


INSERT INTO User(
	userId,
	lastName,
	firstName,
	birthday,
	createdAt,
	updatedAt)
VALUES(
	'1234567',
	'Yamada',
	'Taro',
	'1980-04-15',
	now(),
	now());

INSERT INTO User(
	userId,
	lastName,
	firstName,
	birthday,
	createdAt,
	updatedAt)
VALUES(
	'7654321',
	'Tanaka',
	'Jiro',
	'2003-12-25',
	now(),
	now());

INSERT INTO User(
	userId,
	lastName,
	firstName,
	birthday,
	createdAt,
	updatedAt)
VALUES(
	'8888888',
	'Kimura',
	'Hanako',
	'1978-03-31',
	now(),
	now());
