Why aspire?
seemed like an easy and robust way to wire up central logging, service discovery and integration tests locally, that can also be deployed easily.

why Postgres?
Free/opensource, reliable, scalable. More production-ready than SQLite. Could have considered something non-RDBMS more, but probably not important and postgres probably wins on tooling, documentation etc. to keep momentum

why v1, numerical, redirect
for v1 endpoints I made the choise to use numerical codes, and interpret 'videresending' as a redirect. This was mostly to keep it simple in the beginning, and make sure I stayed on spec, as the one example code was "/1234567".

I considered making a v2 API that would use alphanumerical codes, forward/proxy the request instead of redirect, and possibly do some healthcheck on the target as well.

I descided against proxying, mostly because this is meant to redirect to a website with assets and such, not another API. That means the targets assets with relative paths wouldn't work without extensive rewrites of the returned html, and we don't have to think about stuff like timeouts and sixe of the target etc. 

I considered using base62 encoding for alphanumerical codes, but using Squids meant we avoided sequential codes, discouraging users from trying to discover all codes and targets. 

instead of target healthcheck or requiring only relative path targets, to avoid becoming a phishing gateway, I opted for environment variable with trusted domain of targets.