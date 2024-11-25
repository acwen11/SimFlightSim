
# Copyright (C) 2020-2023 Gabriele Bozzola
#
# This program is free software; you can redistribute it and/or modify it under
# the terms of the GNU General Public License as published by the Free Software
# Foundation; either version 3 of the License, or (at your option) any later
# version.
#
# This program is distributed in the hope that it will be useful, but WITHOUT
# ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
# FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
# details.
#
# You should have received a copy of the GNU General Public License along with
# this program; if not, see <https://www.gnu.org/licenses/>.

import os
import numpy as np

from kuibit import argparse_helper as kah
from kuibit.simdir import SimDir

if __name__ == "__main__":
		desc = f"""{kah.get_program_name()} dumps a 3D grid variable
		resampled to a given chunk layout into a file, and saves layout data
		to a separate file. """

		parser = kah.init_argparse(description=desc)
		parser.add_argument(
				"--variable", default='rho_b', type=str, help="Variable to save."
		)
		parser.add_argument(
				"--iteration",
				type=int,
				default=-1,
				help="Iteration to plot. If -1, the latest.",
		)
		parser.add_argument(
				"--dataout",
				type=str,
				help="Name of the data output files.",
		)
		parser.add_argument(
				"-x0",
				"--origin",
				type=float,
				nargs="+",
		)
		parser.add_argument(
				"-x1",
				"--corner",
				type=float,
				nargs="+",
		)
		parser.add_argument(
				"--numchunks",
				type=int,
				nargs="+",
		)

		args = kah.get_args(parser)

		try:
				os.mkdir(args.dataout)
				print(f"Directory '{directory_name}' created successfully.")
		except FileExistsError:
				print(f"Directory '{directory_name}' already exists.")
		except PermissionError:
				print(f"Permission denied: Unable to create '{directory_name}'.")
		except Exception as e:
				print(f"An error occurred: {e}")

		layoutname = args.dataout + "_pars.txt"
		layoutpath = os.path.join(args.dataout, layoutname)

		iteration = args.iteration
		x0, x1 = args.origin, args.corner
		print(x0)
		print(x1)
		nchunks = args.numchunks
		# chunks hardcoded as 128^3 points, and we want chunk interfaces to share the same points
		shape = 128 * np.array(nchunks) - (np.array(nchunks) - 1)
		print(shape)
		boundsx = (x1[0] - x0[0]) / nchunks[0]
		boundsy = (x1[1] - x0[1]) / nchunks[1]
		boundsz = (x1[2] - x0[2]) / nchunks[2]
		print("Chunk physical bounds: {:f} {:f} {:f}".format(boundsx, boundsy, boundsz))

		assert boundsx == boundsy, "ERROR: chunk physical bounds do not match"
		assert boundsx == boundsz, "ERROR: chunk physical bounds do not match"

		print("Reading variable {:s}".format(args.variable))
		with SimDir(
				args.datadir,
				ignore_symlinks=args.ignore_symlinks,
				pickle_file=args.pickle_file,
		) as sim:
				reader = sim.gridfunctions['xyz']
				var = reader[args.variable]
				print("Read variable")

				if iteration == -1:
						iteration = var.available_iterations[-1]

				print("Reading {:d} and resampling".format(iteration))

				data = var[iteration].to_UniformGridData(shape, x0, x1).data

		for kk in range(nchunks[2]):
			for jj in range(nchunks[1]):
				for ii in range(nchunks[0]):
					# Slice data s.t. adjacent chunks share a point
					imin = ii * 128 - ii
					imax = (ii + 1) * 128 - ii
					jmin = jj * 128 - jj
					jmax = (jj + 1) * 128 - jj
					kmin = kk * 128 - kk
					kmax = (kk + 1) * 128 - kk

					chdata = data[imin:imax, jmin:jmax, kmin:kmax]
					chdata = chdata.reshape(np.prod(chdata.shape)) # flatten to 1d
					outname = args.dataout + "_{:d}{:d}{:d}.txt".format(ii, jj, kk)
					output_path = os.path.join(args.dataout, outname)

					print("Saving to {:s}".format(output_path))
					# TODO: Save as binary
					np.savetxt(output_path, chdata)

		print("Writing layout file")
		with open(layoutpath, 'w') as layoutf:
				layoutf.write("Num chunks: {:d} {:d} {:d}".format(args.numchunks[0], args.numchunks[1], args.numchunks[2]))
				layoutf.write("boundsSize: {:f}".format(boundsx))
