namespace Janrain.Yadis

import System

class AcceptHeader:
    static def Generate(*elements as (object)):
        parts = []
        for element in elements:
            if element isa string:
                qs = "1.0"
                mtype = element
            else:
                mtype, q = element
                qd = cast(decimal, q)
                if qd > 1 or qd <= 0:
                    raise ApplicationException(
			'Invalid preference factor: ${q}')

                qs = qd.ToString()

            parts.Add((qs, mtype))

        parts.Sort()
        chunks = []
        for q, mtype in parts:
            if q == '1.0':
                chunks.Add(mtype)
            else:
                chunks.Add("${mtype}; q=${q}")

        return chunks.Join(", ")


    static def Parse(value as string):
        accept = []
        for chunk in /,/.Split(value):
            parts = [s.Trim() for s in /;/.Split(chunk)]

            mtype as string = parts.Pop(0)

            if '/' not in mtype:
                # This is not a MIME type, so ignore the bad data
                continue

            main, sub = /\//.Split(mtype, 2)

            q = 1.0
            for ext as string in parts:
                if '=' in ext:
                    k, v = /=/.Split(ext, 2)
                    if k == 'q':
                        try:
                            q = Convert.ToDecimal(v)
                            break
                        except FormatException:
                            # Ignore poorly formed q-values
                            pass


            accept.Add((main, sub, q))

        # Sort in reverse order by q
        accept.Sort() do(left as (object), right as (object)):
            l1, l2, l3 as decimal = left
            r1, r2, r3 as decimal = right
            if l3 == r3:
                return 0
            else:
                if l3 < r3:
                    return 1
                else:
                    return -1

        return accept


    static def matchTypes(accept_types, have_types):
        default as decimal
        if accept_types is null:
            # Accept all of them
            default = 1.0
        else:
            default = 0.0

        match_main = {}
        match_sub = {}
        for main, sub, q as decimal in accept_types:
            if main == '*':
                default = Math.Max(default, q)
                continue
            elif sub == '*':
                old_q = match_main[main]
                if old_q is null:
                    old_q = 0.0

                match_main[main] = Math.Max(cast(decimal, old_q), q)
            else:
                key = (main, sub)
                old_q = match_sub[key]
                if old_q is null:
                    old_q = 0.0

                match_sub[key] = Math.Max(cast(decimal, old_q), q)

        accepted_list = []
        order_maintainer = 0
        for mtype in have_types:
            pair = /\//.Split(mtype)
            main, sub = pair
            if pair in match_sub:
                q = match_sub[pair]
            else:
                q = match_main[main]
                if q is null:
                    q = default

            if q is not null:
                x = cast(decimal, q)
                if x != cast(decimal, 0.0):
                    accepted_list.Add((cast(decimal, 1.0) - x,
                                       order_maintainer, x, mtype))
                    order_maintainer += 1


        return [(mtype, q) for x, y, q, mtype in accepted_list]


    static def GetAcceptable(accept_header, have_types):
        accepted = Parse(accept_header)
        preferred = matchTypes(accepted, have_types)
        return [mtype for mtype, _ in preferred]

# for x, y, z in AcceptHeader.Parse(AcceptHeader.Generate(("application/foo", 0.2), ("application/zif", 0.8), ("application/foo", 0.8), "application/bar")):
#     #print x, y, z
#     pass


# acceptable = AcceptHeader.Parse('text/html, text/plain; q=0.5')
# for k, v in AcceptHeader.matchTypes(acceptable, ['text/plain', 'text/html', 'image/jpeg']):
#     print k, v
