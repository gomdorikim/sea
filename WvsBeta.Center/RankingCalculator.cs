using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WvsBeta.Database;

namespace WvsBeta.Center {
	class RankingCalculator {
        public static void StartRankingCalculator()
        {
            MasterThread.Instance.AddRepeatingAction(new MasterThread.RepeatingAction(
                "Ranking Calculator",
                (date) =>
                {
                    CalculateJobs(date);
                    CalculateOverall(date);

                },
                0,
                1 * 60 * 60 * 1000));

            Console.WriteLine("Started!");
        }

		private static void CalculateOverall(DateTime StartTime) {
			Console.Write("Calculating overall rankings... ");

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("SET @rank := @real_rank := 0, @level := @exp := -1;");
			sb.AppendLine("UPDATE characters target");
			sb.AppendLine("	INNER JOIN (");
			sb.AppendLine("		SELECT c.id,");
			sb.AppendLine("			GREATEST(");
			sb.AppendLine("				@rank := IF(level <> 200 AND @level = level AND @exp = exp, @rank, @real_rank + 1),");
			sb.AppendLine("				LEAST(0, @real_rank := @real_rank + 1),");
			sb.AppendLine("				LEAST(0, @level := level),");
			sb.AppendLine("				LEAST(0, @exp := exp)");
			sb.AppendLine("			) AS rank");
			sb.AppendLine("		FROM characters c");
			sb.AppendLine("		LEFT JOIN users u ON u.id = c.userid");
			sb.AppendLine("		WHERE ");
			sb.AppendLine("			u.ban_expire < NOW()");
			sb.AppendLine("			AND u.gm = 0");
			sb.AppendLine("			AND ((c.job = 0 AND c.level > 9) OR (c.job != 0))");
			sb.AppendLine("		ORDER BY");
			sb.AppendLine("			c.level DESC,");
			sb.AppendLine("			c.exp DESC,");
			sb.AppendLine("			c.time_level ASC");
			sb.AppendLine("	) AS source ON source.id = target.id");
			sb.AppendLine("	SET");
			sb.AppendLine("		target.overall_opos = target.overall_cpos,");
			sb.AppendLine("		target.overall_cpos = source.rank;");

			CenterServer.Instance.CharacterDatabase.RunQuery(sb.ToString());

			Console.WriteLine(string.Format("Done calculating Overall Rankings! {0} ms.", Math.Round((DateTime.Now - StartTime).TotalMilliseconds, 0))); // wonder why this appears in center log 7 times over when it's only supposed to happen once per rank type (two total). weird... TODO
		}

		private static void CalculateJobs(DateTime StartTime) {
			for (int i = 0; i < 5; i++) {
				Console.Write("Calculating rankings of job {0}-{1}", i * 100, (i + 1) * 100);

				StringBuilder sb = new StringBuilder();
				sb.AppendLine("SET @rank := @real_rank := 0, @level := @exp := -1;");
				sb.AppendLine("UPDATE characters target");
				sb.AppendLine("	INNER JOIN (");
				sb.AppendLine("		SELECT c.id,");
				sb.AppendLine("			GREATEST(");
				sb.AppendLine("				@rank := IF(level <> 200 AND @level = level AND @exp = exp, @rank, @real_rank + 1),");
				sb.AppendLine("				LEAST(0, @real_rank := @real_rank + 1),");
				sb.AppendLine("				LEAST(0, @level := level),");
				sb.AppendLine("				LEAST(0, @exp := exp)");
				sb.AppendLine("			) AS rank");
				sb.AppendLine("		FROM characters c");
				sb.AppendLine("		LEFT JOIN users u ON u.id = c.userid");
				sb.AppendLine("		WHERE ");
				sb.AppendLine("			u.ban_expire < NOW()");
				sb.AppendLine("			AND u.gm = 0");
				sb.AppendLine("			AND ((c.job = 0 AND c.level > 9) OR (c.job != 0))");
				sb.AppendLine(string.Format("			AND (job DIV 100) = {0}", i));
				sb.AppendLine("		ORDER BY");
				sb.AppendLine("			c.level DESC,");
				sb.AppendLine("			c.exp DESC,");
				sb.AppendLine("			c.time_level ASC");
				sb.AppendLine("	) AS source ON source.id = target.id");
				sb.AppendLine("	SET");
				sb.AppendLine("		target.job_opos = target.job_cpos,");
				sb.AppendLine("		target.job_cpos = source.rank;");

				CenterServer.Instance.CharacterDatabase.RunQuery(sb.ToString());

				Console.WriteLine(string.Format("Done calculating Job Rankings! {0} mseconds.", Math.Round((DateTime.Now - StartTime).TotalMilliseconds, 0)));
			}
		}
	}
}
