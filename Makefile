all:
	docker-compose up --build
down:
	docker-compose down
clean: down
	docker system prune -a --volumes
