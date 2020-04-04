alter table layer
add column NGLFactor double not null;

alter table tubing
add column CoeffWaterAccumulationA double not null,
add column CoeffWaterAccumulationB double not null,
add column CoeffWaterAccumulationC double not null,
add column CoeffGasRateFallingA double not null;
-- Все значения проинициализируются нулями, так что для моделирования нужно задать корректные значения 