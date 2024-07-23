# ArticleReviewRestApi
### REST API's for managing articles and reviews

This project contains two RESTful APIs for managing articles and reviews. These services are built using .NET and MongoDB and are designed to be run in Docker containers.

## Description

ArticleRestApi: Manages articles.
ReviewRestApi: Manages reviews associated with articles.

## Prerequisites
Docker installed on your machine.
.NET SDK installed on your machine.

## Building the APIs

### ArticleRestApi
1.Navigate to the ArticleRestApi directory:

	```cd ArticleRestApi```

2.Build the Docker image:

	```docker build -t articlerestapi .```
 
### ReviewRestApi
1.Navigate to the ReviewRestApi directory:

	```cd ReviewRestApi```
 
2.Build the Docker image:

	docker build -t reviewrestapi .```
 
## Running the Services
### ArticleRestApi

Run the ArticleRestApi service using Docker:

	```docker run -it --rm -p 5002:8080 --name articlerestapi_container articlerestapi```

Now you can access this api by browsing http://host.docker.internal:5002/index.html

### ReviewRestApi

Run the ReviewRestApi service using Docker:

	```docker run -it --rm -p 5001:8080 --name reviewrestapi_container reviewrestapi```

Now you can access this api by browsing http://host.docker.internal:5001/index.html

## Authentication

Both services are protected with JWT authentication. To access the endpoints, you need to authenticate using the following credentials:

**Username:** ***user***

**Password:** ***pass***

### How to Authenticate

Obtain a JWT token by sending a POST request to the authentication endpoint. 

![auth1](https://github.com/user-attachments/assets/cc42fd64-50fe-43a1-9206-1446a22ab405)

The response will include a JWT token. Use this token to access the protected endpoints by including it in the Authorization header:

*You should put "Bearer" and then a space before pasting your key;*

Authorization: Bearer [your-token]

![auth2](https://github.com/user-attachments/assets/8b3c9913-cfce-4a9c-b452-c9d8ae83581f)

Now when you click on Authorize button, you can send requests to all enpoints. otherwise you will get an Unauthorized error.

## API Endpoints

### ArticleRestApi
* GET /api/v1/articles: Get all articles.
* GET /api/v1/articles/{id}: Get a specific article by ID.
* POST /api/v1/articles: Create a new article.
* PUT /api/v1/articles: Update an existing article.
* DELETE /api/v1/articles/{id}: Delete an article by ID.

### ReviewRestApi
* GET /api/v1/reviews: Get all reviews.
* GET /api/v1/reviews/{id}: Get a specific review by ID.
* POST /api/v1/reviews: Create a new review.
* PUT /api/v1/reviews: Update an existing review.
* DELETE /api/v1/reviews/{id}: Delete a review by ID.

### Error Handling

The APIs include robust error handling and return meaningful error messages for various scenarios, including:

* Invalid ID format (must be a 24-digit hex string).
* Resource not found.
* Internal server errors.
* Contributing
* Contributions are welcome! Please submit a pull request or open an issue to discuss your changes.
