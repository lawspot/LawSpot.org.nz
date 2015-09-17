update Category set Name = 'Charitable Organisations', Slug = 'charities' where Name = 'Trusts, Societies, Charitable Organisations'

update Category set Name = 'Health and ACC', Slug = 'health-acc' where Name = 'Health, Safety and ACC'

insert Category (Name, Slug) values ('Legal Aid', 'legal-aid')
insert Category (Name, Slug) values ('Neighbours', 'neighbours')

delete Category where Name = 'Negligence'
delete Category where Name = 'Nuisance'

update Category set Name = 'Tenancy', Slug = 'tenancy' where Name = 'Tenancy/Real Estate'

insert Category (Name, Slug) values ('Welfare/Benefits', 'welfare-benefits')
insert Category (Name, Slug) values ('Wills/Estates', 'wills-estates')