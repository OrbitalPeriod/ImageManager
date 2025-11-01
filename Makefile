IMAGES = animetagger imagemanager frontend
REGISTRY = registry.orbitalperiod.net

.PHONY: build push all

build:
	docker-compose build

push:
	@for img in $(IMAGES); do \
		docker tag $$img $(REGISTRY)/$$img:latest; \
		docker push $(REGISTRY)/$$img:latest; \
	done

all: build push
