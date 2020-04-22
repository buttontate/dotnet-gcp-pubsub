create table item
(
	id serial not null,
	created timestamp not null,
	updated timestamp,
	upc varchar not null,
	description varchar not null
);

create unique index item_id_uindex
	on item (id);

alter table item
	add constraint item_pk
		primary key (id);

