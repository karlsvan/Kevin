Why aspire?
seemed like an easy and robust way to wire up central logging, service discovery and healthchecks locally, that can also be deployed easily 

why Postgres?
Free/opensource, reliable, scalable. More production-ready than SQLite. Could have considered something non-RDBMS more, but probably not important and postgres probably wins on tooling, documentation etc. to keep momentum

why v1, numerical, redirect
for v1 endpoints I ade th choise to use numerical codes, and interpret 'videresending' as a redirect. This was mostly to keep it simple in the beginning, and make sure I stayed on spec, as the one code example was "/1234567". 

v2, alphanumerical base62, forward, url-livelness check?s