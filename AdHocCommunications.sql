if object_id('command_queue') is not null drop table command_queue
go

create table command_queue
(
	command_queue_id uniqueidentifier not null primary key,
	time_queued datetime not null,
	command_type varchar(50) not null,
	template_name varchar(255) not null,
	template_fields varchar(max) not null,
)
go

create index command_queue_idx1 on command_queue(command_type, time_queued, command_queue_id);
go

/* TODO: 
* Add type priority table 
* also index on this 
* and TimeQueued 
* add field to serialise template fields into */

