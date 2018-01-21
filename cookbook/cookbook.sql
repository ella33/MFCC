CREATE DATABASE ingredients;
USE ingredients;

CREATE TABLE units(id INT NOT NULL AUTO_INCREMENT,
                  name VARCHAR(80) NOT NULL,
                  PRIMARY KEY(id));

CREATE TABLE ingredient(id INT NOT NULL AUTO_INCREMENT,
                       name VARCHAR(80) NOT NULL,
                       unitId INT NOT NULL,
                       PRIMARY KEY(id),
                       FOREIGN KEY(unitId) REFERENCES units(id));

CREATE DATABASE recipes;
USE recipes;

CREATE TABLE recipeCategory(id INT NOT NULL AUTO_INCREMENT,
                           name VARCHAR(80),
                           PRIMARY KEY(id));

CREATE TABLE recipe(id INT NOT NULL AUTO_INCREMENT,
                   title VARCHAR(80) NOT NULL,
                   categoryId INT NOT NULL,
                    PRIMARY KEY(id),
                    FOREIGN KEY(categoryId) REFERENCES recipeCategory(id));

                           
CREATE TABLE recipeIngredients(id INT NOT NULL AUTO_INCREMENT,
                              recipeId INT NOT NULL,
                              ingredientId INT NOT NULL,
                              PRIMARY KEY(id),
                              FOREIGN KEY(recipeId) REFERENCES recipe(id),
                              FOREIGN KEY(ingredientId) REFERENCES ingredients.ingredient(id));

