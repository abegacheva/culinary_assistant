using Culinary_Assistant.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;

namespace Culinary_Assistant
{
    public class DatabaseHelper
    {
        private readonly string connectionString =
            ConfigurationManager
            .ConnectionStrings["CulinaryDb"]
            .ConnectionString;

        public void SaveLikedRecipe(
            string recipeKey,
            string title,
            double calories,
            double protein,
            double carbs,
            string instructions,
            string ingredients)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    INSERT INTO liked_recipes
                    (
                        recipe_key,
                        title,
                        calories,
                        protein,
                        carbs,
                        instructions,
                        ingredients
                    )
                    VALUES
                    (
                        @key,
                        @title,
                        @calories,
                        @protein,
                        @carbs,
                        @instructions,
                        @ingredients
                    )
                    ON CONFLICT (recipe_key)
                    DO UPDATE SET
                        title = EXCLUDED.title,
                        calories = EXCLUDED.calories,
                        protein = EXCLUDED.protein,
                        carbs = EXCLUDED.carbs,
                        instructions = EXCLUDED.instructions,
                        ingredients = EXCLUDED.ingredients;
                ";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "key",
                        recipeKey ?? ""
                    );

                    cmd.Parameters.AddWithValue(
                        "title",
                        title ?? ""
                    );

                    cmd.Parameters.AddWithValue(
                        "calories",
                        calories
                    );

                    cmd.Parameters.AddWithValue(
                        "protein",
                        protein
                    );

                    cmd.Parameters.AddWithValue(
                        "carbs",
                        carbs
                    );

                    cmd.Parameters.AddWithValue(
                        "instructions",
                        instructions ?? ""
                    );

                    cmd.Parameters.AddWithValue(
                        "ingredients",
                        ingredients ?? ""
                    );

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SaveLikedRecipeRu(
            string recipeKey,
            string titleRu,
            string instructionsRu,
            string ingredientsRu)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    INSERT INTO liked_recipes_ru
                    (
                        recipe_key,
                        title_ru,
                        instructions_ru,
                        ingredients_ru
                    )
                    VALUES
                    (
                        @key,
                        @title_ru,
                        @instructions_ru,
                        @ingredients_ru
                    )
                    ON CONFLICT (recipe_key)
                    DO UPDATE SET
                        title_ru = EXCLUDED.title_ru,
                        instructions_ru = EXCLUDED.instructions_ru,
                        ingredients_ru = EXCLUDED.ingredients_ru;
                ";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "key",
                        recipeKey ?? ""
                    );

                    cmd.Parameters.AddWithValue(
                        "title_ru",
                        titleRu ?? ""
                    );

                    cmd.Parameters.AddWithValue(
                        "instructions_ru",
                        instructionsRu ?? ""
                    );

                    cmd.Parameters.AddWithValue(
                        "ingredients_ru",
                        ingredientsRu ?? ""
                    );

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public DataTable GetLikedRecipes()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    SELECT *
                    FROM liked_recipes
                    ORDER BY id DESC;
                ";

                using (var adapter =
                    new NpgsqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();

                    adapter.Fill(dt);

                    return dt;
                }
            }
        }

        public DataTable GetLikedRecipesRu()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    SELECT *
                    FROM liked_recipes_ru
                    ORDER BY id DESC;
                ";

                using (var adapter =
                    new NpgsqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();

                    adapter.Fill(dt);

                    return dt;
                }
            }
        }

        public void RemoveLikedRecipe(string recipeKey)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string sql1 =
                    "DELETE FROM liked_recipes WHERE recipe_key=@k";

                using (var cmd =
                    new NpgsqlCommand(sql1, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "k",
                        recipeKey
                    );

                    cmd.ExecuteNonQuery();
                }

                string sql2 =
                    "DELETE FROM liked_recipes_ru WHERE recipe_key=@k";

                using (var cmd =
                    new NpgsqlCommand(sql2, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "k",
                        recipeKey
                    );

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool IsRecipeLiked(string recipeKey)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    SELECT COUNT(*)
                    FROM liked_recipes
                    WHERE recipe_key=@k;
                ";

                using (var cmd =
                    new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "k",
                        recipeKey
                    );

                    return Convert.ToInt32(
                        cmd.ExecuteScalar()
                    ) > 0;
                }
            }
        }

        public void SaveRecommendations(List<Recipe> recipes)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var clear =
                    new NpgsqlCommand(
                        "DELETE FROM recommended_recipes",
                        conn))
                {
                    clear.ExecuteNonQuery();
                }

                foreach (var r in recipes)
                {
                    string sql = @"
                        INSERT INTO recommended_recipes
                        (
                            recipe_key,
                            title,
                            ingredients,
                            instructions,
                            calories,
                            protein,
                            carbs,
                            fat
                        )
                        VALUES
                        (
                            @key,
                            @title,
                            @ingredients,
                            @instructions,
                            @calories,
                            @protein,
                            @carbs,
                            @fat
                        );
                    ";

                    using (var cmd =
                        new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue(
                            "key",
                            r.RecipeKey ?? ""
                        );

                        cmd.Parameters.AddWithValue(
                            "title",
                            r.Title ?? ""
                        );

                        cmd.Parameters.AddWithValue(
                            "ingredients",
                            r.Ingredients ?? ""
                        );

                        cmd.Parameters.AddWithValue(
                            "instructions",
                            string.Join(
                                "\n",
                                r.Instructions ?? new List<string>()
                            )
                        );

                        cmd.Parameters.AddWithValue(
                            "calories",
                            r.Nutrition?.Calories ?? 0
                        );

                        cmd.Parameters.AddWithValue(
                            "protein",
                            r.Nutrition?.Protein ?? 0
                        );

                        cmd.Parameters.AddWithValue(
                            "carbs",
                            r.Nutrition?.TotalCarbohydrates ?? 0
                        );

                        cmd.Parameters.AddWithValue(
                            "fat",
                            r.Nutrition?.TotalFat ?? 0
                        );

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void SaveRecommendationsRu(List<Recipe> recipes)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var clear =
                    new NpgsqlCommand(
                        "DELETE FROM recommended_recipes_ru",
                        conn))
                {
                    clear.ExecuteNonQuery();
                }

                foreach (var r in recipes)
                {
                    string sql = @"
                        INSERT INTO recommended_recipes_ru
                        (
                            recipe_key,
                            title_ru,
                            ingredients_ru,
                            instructions_ru
                        )
                        VALUES
                        (
                            @key,
                            @title_ru,
                            @ingredients_ru,
                            @instructions_ru
                        );
                    ";

                    using (var cmd =
                        new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue(
                            "key",
                            r.RecipeKey ?? ""
                        );

                        cmd.Parameters.AddWithValue(
                            "title_ru",
                            r.TitleRu ?? ""
                        );

                        cmd.Parameters.AddWithValue(
                            "ingredients_ru",
                            r.IngredientsRu ?? ""
                        );

                        cmd.Parameters.AddWithValue(
                            "instructions_ru",
                            string.Join(
                                "\n",
                                r.InstructionsRu ?? new List<string>()
                            )
                        );

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public DataTable GetRecommendations()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    SELECT *
                    FROM recommended_recipes
                    ORDER BY id DESC;
                ";

                using (var adapter =
                    new NpgsqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();

                    adapter.Fill(dt);

                    return dt;
                }
            }
        }

        public DataTable GetRecommendationsRu()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                    SELECT *
                    FROM recommended_recipes_ru
                    ORDER BY id DESC;
                ";

                using (var adapter =
                    new NpgsqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();

                    adapter.Fill(dt);

                    return dt;
                }
            }
        }
    }
}
